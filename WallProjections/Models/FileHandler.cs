using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WallProjections.Models.Interfaces;
using static WallProjections.Models.Interfaces.IFileHandler;

namespace WallProjections.Models;

/// <summary>
/// Class for all file handling functions for program (includes Config handling methods)
/// </summary>
public class FileHandler : IFileHandler
{
    /// <summary>
    /// A logger for this class.
    /// </summary>
    private readonly ILogger _logger;

    /// <inheritdoc cref="FileHandler" />
    public FileHandler(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<FileHandler>();
    }

    /// <inheritdoc />
    public IConfig ImportConfig(string zipPath)
    {
        using var configBuilder = new ConfigBuilder(_logger);
        _logger.LogTrace("Builder created");

        configBuilder.ImportConfig(zipPath);
        _logger.LogTrace("Config imported");

        configBuilder.Commit();
        _logger.LogTrace("Config committed");

        var config = LoadConfig();
        _logger.LogInformation("Imported config from {ZipPath}", zipPath);
        return config;
    }

    /// <inheritdoc />
    public bool ExportConfig(string exportPath)
    {
        try
        {
            if (File.Exists(exportPath))
            {
                _logger.LogInformation("Deleting existing file at {ExportPath}", exportPath);
                File.Delete(exportPath);
            }

            ZipFile.CreateFromDirectory(CurrentConfigFolderPath, exportPath);
            _logger.LogInformation("Exported config to {ExportPath}", exportPath);
            return true;
        }
        catch (DirectoryNotFoundException e)
        {
            _logger.LogError(e, "Could not find directory for config at {ExportPath}", exportPath);
            throw new ConfigNotImportedException(e);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            _logger.LogError(e, "Could not export config to {ExportPath}", exportPath);
            throw new ConfigIOException(exportPath, e);
        }
    }


    /// <inheritdoc />
    public bool SaveConfig(IConfig config)
    {
        using var configBuilder = new ConfigBuilder(_logger);
        _logger.LogTrace("Builder created");

        configBuilder.AddHomographyMatrix(config.HomographyMatrix);
        _logger.LogTrace("Homography matrix added");

        foreach (var hotspot in config.Hotspots)
            configBuilder.AddHotspot(hotspot);

        configBuilder.GenerateConfigFile();
        _logger.LogTrace("Config file generated");

        configBuilder.Commit();
        _logger.LogTrace("Config committed");

        return true;
    }

    /// <inheritdoc />
    public IConfig LoadConfig()
    {
        var configLocation = CurrentConfigFilePath;
        try
        {
            using var configFile = File.OpenRead(configLocation);
            _logger.LogTrace("Config file at {ConfigLocation} opened", configLocation);

            var config = JsonSerializer.Deserialize<Config>(configFile);
            if (config is null)
            {
                // Edge case if file contains "null"
                _logger.LogError("Config file contains null value");
                throw new ConfigInvalidException(new Exception("File contains null value"));
            }

            _logger.LogInformation("Loaded config from {ConfigLocation}", configLocation);
            return config;
        }
        catch (DirectoryNotFoundException e)
        {
            _logger.LogError(e, "Could not find directory for config at {ConfigLocation}", configLocation);
            throw new ConfigNotImportedException(e);
        }
        catch (FileNotFoundException e)
        {
            _logger.LogError(e, "Could not find config file at {ConfigLocation}", configLocation);
            throw new ConfigInvalidException(e);
        }
        catch (IOException e)
        {
            _logger.LogError(e, "Could not read config file at {ConfigLocation}", configLocation);
            throw new ConfigIOException(e);
        }
        // When JSON config is not in a valid format.
        catch (Exception e) when (e is JsonException or ArgumentNullException)
        {
            _logger.LogError(e, "Config file at {ConfigLocation} is not a valid config", configLocation);
            throw new ConfigInvalidException(e);
        }
    }

    /// <summary>
    /// Builds and saves a new config.
    /// </summary>
    /// <remarks>This class is not thread safe!</remarks>
    private class ConfigBuilder : IDisposable
    {
        /// <summary>
        /// <see cref="FileHandler" />'s logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Homography matrix for config
        /// </summary>
        private double[,]? _homographyMatrix;

        /// <summary>
        /// List of hotspots to be stored in the new config.
        /// </summary>
        private readonly List<Hotspot> _hotspots;

        /// <summary>
        /// Store if a config has been imported from a package.
        /// </summary>
        private bool _configImported;

        /// <summary>
        /// Store if config file has been generated for new config
        /// </summary>
        private bool _configFileGenerated;

        /// <summary>
        /// Stores whether the config has been committed to storage.
        /// </summary>
        private bool _configCommitted;

        /// <summary>
        /// Calculated property for if a new config is being made.
        /// </summary>
        /// <returns>If new config is being made.</returns>
        private bool IsNewConfig() => _hotspots.Count != 0 || _homographyMatrix is not null;

        /// <summary>
        /// Constructor for config builder. Creates the temporary folder for saving.
        /// </summary>
        /// <exception cref="ConfigIOException">
        /// Thrown when <see cref="IFileHandler.TempConfigFolderPath"/> could not be modified
        /// due to permission/access or blocking issues.
        /// </exception>
        public ConfigBuilder(ILogger logger)
        {
            _logger = logger;
            try
            {
                // Reset/Create temporary config folder
                if (Directory.Exists(TempConfigFolderPath))
                {
                    _logger.LogTrace(
                        "Deleting existing temporary config folder at {TempConfigFolderPath}", TempConfigFolderPath
                    );
                    Directory.Delete(TempConfigFolderPath, true);
                }

                Directory.CreateDirectory(TempConfigFolderPath);
                _logger.LogTrace(
                    "Created temporary config folder at {TempConfigFolderPath}", TempConfigFolderPath
                );

                _hotspots = new List<Hotspot>();
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                _logger.LogError(e,
                    "Could not create temporary config folder at {TempConfigFolderPath}", TempConfigFolderPath
                );
                throw new ConfigIOException(TempConfigFolderPath, e);
            }
        }

        /// <summary>
        /// Import a config from a package file.
        /// </summary>
        /// <param name="zipPath">Path to the config package file.</param>
        /// <exception cref="InvalidOperationException">If a new config has already started being created.</exception>
        /// <exception cref="ExternalFileReadException">If the package does not exist or is not accessible.</exception>
        /// <exception cref="ConfigPackageFormatException">If config package is not a valid package.</exception>
        public void ImportConfig(string zipPath)
        {
            // Can only import if a new config isn't being created
            if (IsNewConfig())
            {
                _logger.LogError("Cannot import when a new config is being created");
                throw new InvalidOperationException("Cannot import when a new config is being created");
            }

            _configImported = true;
            try
            {
                _logger.LogTrace(
                    "Extracting config package from {ZipPath} to {TempConfigFolderPath}",
                    zipPath,
                    TempConfigFolderPath
                );
                ZipFile.ExtractToDirectory(zipPath, TempConfigFolderPath);
                _logger.LogInformation("Config package extracted to {TempConfigFolderPath}", TempConfigFolderPath);
            }
            catch (Exception e) when (e is UnauthorizedAccessException or IOException)
            {
                _logger.LogError(e, "Could not extract config package from {ZipPath}", zipPath);
                throw new ExternalFileReadException(zipPath, e);
            }
            catch (InvalidDataException e)
            {
                _logger.LogError(e, "Config package at {ZipPath} is not a valid package", zipPath);
                throw new ConfigPackageFormatException(Path.GetFileName(zipPath), e);
            }
        }

        /// <summary>
        /// Add the homography matrix to be stored inside the config
        /// </summary>
        /// <param name="homographyMatrix">The homography matrix to be stored in the config</param>
        /// <exception cref="InvalidOperationException">
        ///     If config has already been imported, or if a config file has already been generated for new config.
        /// </exception>
        public void AddHomographyMatrix(double[,] homographyMatrix)
        {
            PerformGeneralChecks();

            // Cannot set homography matrix multiple times
            if (_homographyMatrix is not null)
            {
                _logger.LogError("Homography matrix already added");
                throw new InvalidOperationException("Homography matrix already added");
            }

            _homographyMatrix = homographyMatrix;
            _logger.LogInformation("Added homography matrix to new config");
        }

        /// <summary>
        /// Adds a hotspot to the new config.
        /// </summary>
        /// <param name="hotspot">Hotspot to resolve/add to new config.</param>
        /// <exception cref="InvalidOperationException">
        ///     If config has already been imported, or if a config file has already been generated for new config.
        /// </exception>
        /// <exception cref="ConfigDuplicateFileException">If a file with the same resolved name is already imported.</exception>
        /// <exception cref="ExternalFileReadException">If one of the files to resolve cannot be read.</exception>
        public void AddHotspot(Hotspot hotspot)
        {
            PerformGeneralChecks();

            // Cannot add hotspot if hotspot with the same id already exists
            if (_hotspots.Select(h => h.Id).Contains(hotspot.Id))
            {
                _logger.LogError("Cannot have two hotspots with the same id");
                throw new ArgumentException("Cannot have two hotspots with the same id", nameof(hotspot));
            }

            _hotspots.Add(new Hotspot(
                hotspot.Id,
                hotspot.Position,
                hotspot.Title,
                ResolveFile(hotspot.DescriptionPath, "text", hotspot.Id),
                new List<string>(ResolveFiles(hotspot.ImagePaths, "image", hotspot.Id)).ToImmutableList(),
                new List<string>(ResolveFiles(hotspot.VideoPaths, "video", hotspot.Id)).ToImmutableList()
            ));
            _logger.LogInformation("Added hotspot {HotspotId} to new config", hotspot.Id);
        }

        /// <summary>
        /// Generates the JSON file from the current set of hotspots added with <see cref="AddHotspot"/>
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If no homography matrix has been added, a config has already been imported,
        /// or a config file has already been generated.
        /// </exception>
        /// <exception cref="ConfigIOException">If there is an issue saving the generated config file to disk.</exception>
        public void GenerateConfigFile()
        {
            PerformGeneralChecks();

            if (_homographyMatrix is null)
            {
                _logger.LogError("Must have homography matrix");
                throw new InvalidOperationException("Must have homography matrix");
            }

            _configFileGenerated = true;
            var newConfig = new Config(_homographyMatrix, _hotspots);

            try
            {
#if RELEASE
                var configString = JsonSerializer.Serialize(newConfig);
#else
                var configString = JsonSerializer.Serialize(newConfig, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
#endif
                _logger.LogTrace("Config successfully serialized");

                using var configFile = new StreamWriter(TempConfigFilePath);
                configFile.Write(configString);
                _logger.LogInformation("Config file generated at {TempConfigFilePath}", TempConfigFilePath);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or SecurityException)
            {
                _logger.LogError(e, "Could not save config file to {TempConfigFilePath}", TempConfigFilePath);
                throw new ConfigIOException(TempConfigFilePath, e);
            }
        }

        /// <summary>
        /// Commits the new config and overwrites the old config.
        /// </summary>
        /// <exception cref="InvalidOperationException">If config has not yet been imported or created</exception>
        /// <exception cref="ConfigIOException">If a permissions/blocking error occurs during the commit</exception>
        public void Commit()
        {
            if (!(IsNewConfig() || _configImported))
            {
                _logger.LogError("Must create or import config before committing");
                throw new InvalidOperationException("Must create or import config before committing");
            }

            try
            {
                // Backup current config until save is complete
                if (Directory.Exists(BackupConfigFolderPath))
                {
                    _logger.LogTrace(
                        "Deleting existing backup config folder at {BackupConfigFolderPath}", BackupConfigFolderPath
                    );
                    Directory.Delete(BackupConfigFolderPath, true);
                }

                if (Directory.Exists(CurrentConfigFolderPath))
                {
                    _logger.LogTrace(
                        "Moving current config folder at {CurrentConfigFolderPath} to {BackupConfigFolderPath}",
                        CurrentConfigFolderPath,
                        BackupConfigFolderPath
                    );
                    Directory.Move(CurrentConfigFolderPath, BackupConfigFolderPath);
                }

                _logger.LogTrace(
                    "Moving temporary config folder at {TempConfigFolderPath} to {CurrentConfigFolderPath}",
                    TempConfigFolderPath,
                    CurrentConfigFolderPath
                );
                Directory.Move(TempConfigFolderPath, CurrentConfigFolderPath);
                _configCommitted = true;
                _logger.LogInformation("Config committed");
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or SecurityException)
            {
                _logger.LogError(e, "Could not commit config to {CurrentConfigFolderPath}", CurrentConfigFolderPath);
                throw new ConfigIOException(e);
            }
            finally
            {
                // If not committed and a backup exists, move backup to become current.
                if (!_configCommitted && Directory.Exists(BackupConfigFolderPath))
                {
                    _logger.LogWarning("Rolling back config to previous state");
                    if (Directory.Exists(CurrentConfigFolderPath))
                    {
                        _logger.LogTrace(
                            "Deleting current config folder at {CurrentConfigFolderPath}", CurrentConfigFolderPath
                        );
                        Directory.Delete(CurrentConfigFolderPath, true);
                    }

                    _logger.LogTrace(
                        "Moving backup config folder at {BackupConfigFolderPath} to {CurrentConfigFolderPath}",
                        BackupConfigFolderPath,
                        CurrentConfigFolderPath
                    );
                    Directory.Move(BackupConfigFolderPath, CurrentConfigFolderPath);
                    _logger.LogInformation("Config successfully rolled back to previous state");
                }
            }
        }

        /// <summary>
        /// Resolves file from current config if path is relative and copies file to temporary config.
        /// </summary>
        /// <param name="file">File to resolve</param>
        /// <param name="type">Type of file to resolve</param>
        /// <param name="id">Id of current hotspot</param>
        /// <param name="i">Current number of item (optional)</param>
        /// <returns>Paths of resolved files.</returns>
        /// <exception cref="ConfigDuplicateFileException">If a file with the same resolved name is already imported.</exception>
        /// <exception cref="ExternalFileReadException">If the file to resolve cannot be read.</exception>
        /// <exception cref="ConfigIOException">If there is an issue accessing original file or saving new file.</exception>
        private string ResolveFile(string file, string type, int id, int? i = null)
        {
            var extension = Path.GetExtension(file);
            var newFileName = i is null
                ? $"{type}_{id}{extension}"
                : $"{type}_{id}_{i}{extension}";

            string resolvedFilePath;
            string newFilePath;
            try
            {
                newFilePath = Path.Combine(TempConfigFolderPath, newFileName);
                resolvedFilePath = file.IsInConfig()
                    ? Path.Combine(CurrentConfigFolderPath, file)
                    : file;
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e, "Could not resolve file path");
                throw new ExternalFileReadException(file, e);
            }

            try
            {
                _logger.LogTrace(
                    "Copying file from {ResolvedFilePath} to {NewFilePath}", resolvedFilePath, newFilePath
                );
                File.Copy(resolvedFilePath, newFilePath);
                _logger.LogInformation("File copied to {NewFilePath}", newFilePath);
                return newFileName;
            }
            catch (FileNotFoundException e)
            {
                _logger.LogError(e, "Could not find file to resolve at {ResolvedFilePath}", resolvedFilePath);
                throw new ExternalFileReadException(file, e);
            }
            catch (IOException e)
            {
                if (File.Exists(newFilePath))
                {
                    _logger.LogError(e, "File with same name already exists at {NewFilePath}", newFilePath);
                    throw new ConfigDuplicateFileException(newFileName, e);
                }

                _logger.LogError(e, "Could not copy file to {NewFilePath}", newFilePath);
                throw new ConfigIOException(file, e);
            }
        }

        /// <summary>
        /// Resolves files from current config if path is relative or else an external source and copies files to
        /// temporary config.
        /// </summary>
        /// <param name="files">Files to resolve</param>
        /// <param name="type">Type of the files to resolve</param>
        /// <param name="id">Id of current hotspot</param>
        /// <returns>Paths of resolved files.</returns>
        /// <exceptions>See <see cref="ResolveFile"/></exceptions>
        private IEnumerable<string> ResolveFiles(IEnumerable<string> files, string type, int id) =>
            files.Select((path, i) => ResolveFile(path, type, id, i));

        /// <summary>
        /// Performs general checks about the state of the builder:
        /// <ul>
        ///     <li>Cannot create new config if config has been imported</li>
        ///     <li>Cannot update information if a config file has already been generated</li>
        /// </ul>
        /// </summary>
        /// <exception cref="InvalidOperationException">If general checks fail</exception>
        private void PerformGeneralChecks()
        {
            // Cannot create new config if config has been imported
            if (_configImported)
            {
                _logger.LogError("Config already imported");
                throw new InvalidOperationException("Config already imported");
            }

            // Cannot update information if a config file has already been generated
            if (_configFileGenerated)
            {
                _logger.LogError("Config file already generated");
                throw new InvalidOperationException("Config file already generated");
            }
        }

        /// <summary>
        /// Delete temporary config folder and backup
        /// </summary>
        public void Dispose()
        {
            if (Directory.Exists(TempConfigFolderPath))
            {
                Directory.Delete(TempConfigFolderPath, true);
                _logger.LogTrace("Deleted temporary config folder at {TempConfigFolderPath}", TempConfigFolderPath);
            }

            if (Directory.Exists(BackupConfigFolderPath))
            {
                Directory.Delete(BackupConfigFolderPath, true);
                _logger.LogTrace("Deleted backup config folder at {BackupConfigFolderPath}", BackupConfigFolderPath);
            }
        }
    }
}

/// <summary>
/// Extension methods for media file paths.
/// </summary>
internal static class PathExtensions
{
    /// <summary>
    /// Checks if the file is in the <see cref="IFileHandler.CurrentConfigFolderPath">config folder</see>.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    /// <seealso cref="IsNotInConfig" />
    // ReSharper disable once MemberCanBePrivate.Global
    public static bool IsInConfig(this string path) => path.StartsWith(CurrentConfigFolderPath) && File.Exists(path);

    /// <summary>
    /// Checks if the file is not in the <see cref="IFileHandler.CurrentConfigFolderPath">config folder</see>.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    /// <seealso cref="IsInConfig" />
    public static bool IsNotInConfig(this string path) => !path.IsInConfig() && File.Exists(path);
}
