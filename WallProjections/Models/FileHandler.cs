using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Text.Json;
using WallProjections.Models.Interfaces;
using static WallProjections.Models.Interfaces.IFileHandler;

namespace WallProjections.Models;

/// <summary>
/// Class for all file handling functions for program (includes Config handling methods)
/// </summary>
/// TODO: Add logging for any errors found during file handling
public class FileHandler : IFileHandler
{
    /// <inheritdoc />
    public IConfig ImportConfig(string zipPath)
    {
        using var configBuilder = new ConfigBuilder();

        configBuilder.ImportConfig(zipPath);

        configBuilder.Commit();

        return LoadConfig();
    }

    /// <inheritdoc />
    public bool ExportConfig(string exportPath)
    {
        try
        {
            if (File.Exists(exportPath))
                File.Delete(exportPath);

            ZipFile.CreateFromDirectory(CurrentConfigFolderPath, exportPath);
            return true;
        }
        catch (DirectoryNotFoundException e)
        {
            throw new ConfigNotImportedException(e);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            throw new ConfigIOException(exportPath, e);
        }
    }


    /// <inheritdoc />
    public bool SaveConfig(IConfig config)
    {
        using var configBuilder = new ConfigBuilder();

        configBuilder.AddHomographyMatrix(config.HomographyMatrix);

        foreach (var hotspot in config.Hotspots)
        {
            configBuilder.AddHotspot(hotspot);
        }

        configBuilder.GenerateConfigFile();
        configBuilder.Commit();

        return true;
    }

    /// <inheritdoc />
    public IConfig LoadConfig()
    {
        var configLocation = CurrentConfigFilePath;

        try
        {
            using var configFile = File.OpenRead(configLocation);
            return JsonSerializer.Deserialize<Config>(configFile) ??
                   // Edge case if file contains "null"
                   throw new ConfigInvalidException(new Exception("File contains null value"));

        }
        catch (DirectoryNotFoundException e)
        {
            throw new ConfigNotImportedException(e);
        }
        catch (FileNotFoundException e)
        {
            throw new ConfigInvalidException(e);
        }
        catch (IOException e)
        {
            throw new ConfigIOException(e);
        }
        // When JSON config is not in a valid format.
        catch (Exception e) when (e is JsonException or ArgumentNullException)
        {
            throw new ConfigInvalidException(e);
        }
    }

    /// <summary>
    /// Builds and saves a new config.
    /// </summary>
    private class ConfigBuilder : IDisposable
    {
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
        /// <exception cref="ConfigIOException">
        ///     Thrown if the <see cref="IFileHandler.TempConfigFolderPath"/> could not be modified
        ///     due to permission/access or blocking issues.
        /// </exception>
        /// </summary>
        public ConfigBuilder()
        {
            try
            {
                // Reset/Create temporary config folder
                if (Directory.Exists(TempConfigFolderPath))
                    Directory.Delete(TempConfigFolderPath, true);

                Directory.CreateDirectory(TempConfigFolderPath);

                _hotspots = new List<Hotspot>();
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
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
                throw new InvalidOperationException("Cannot import when a new config is being created");

            _configImported = true;
            try
            {
                ZipFile.ExtractToDirectory(zipPath, TempConfigFolderPath);
            }
            catch (Exception e) when (e is UnauthorizedAccessException or IOException)
            {
                throw new ExternalFileReadException(zipPath, e);
            }
            catch (InvalidDataException e)
            {
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
            // Cannot create new config if config has been imported
            if (_configImported)
                throw new InvalidOperationException("Config already imported");

            // Cannot update information if a config file has already been generated
            if (_configFileGenerated)
                throw new InvalidOperationException("Config file already generated");

            // Cannot set homography matrix multiple times
            if (_homographyMatrix is not null)
                throw new InvalidOperationException("Homography matrix already added");

            _homographyMatrix = homographyMatrix;
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
            // Cannot create new config if config has been imported
            if (_configImported)
                throw new InvalidOperationException("Config already imported");

            // Cannot update information if a config file has already been generated
            if (_configFileGenerated)
                throw new InvalidOperationException("Config file already generated");

            // Cannot add hotspot if hotspot with the same id already exists
            if (_hotspots.Select(h => h.Id).Contains(hotspot.Id))
                throw new ArgumentException("Cannot have two hotspots with the same id", nameof(hotspot));

            _hotspots.Add(new Hotspot(
                hotspot.Id,
                hotspot.Position,
                hotspot.Title,
                ResolveFile(hotspot.DescriptionPath, "text", hotspot.Id),
                new List<string>(ResolveFiles(hotspot.ImagePaths, "image", hotspot.Id)).ToImmutableList(),
                new List<string>(ResolveFiles(hotspot.VideoPaths, "video", hotspot.Id)).ToImmutableList()
            ));
        }

        /// <summary>
        /// Generates the JSON file from the current set of hotspots added with <see cref="AddHotspot"/>
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     If no homography matrix has been added, a config has
        ///     already been imported, or a config file has already been generated.
        /// </exception>
        /// <exception cref="ConfigIOException">If there is an issue saving the generated config file to disk.</exception>
        public void GenerateConfigFile()
        {
            // Cannot create new config if config has been imported
            if (_configImported)
                throw new InvalidOperationException("Config already imported");

            if (_homographyMatrix is null)
                throw new InvalidOperationException("Must have homography matrix");

            if (_configFileGenerated)
                throw new InvalidOperationException("Config file already generated for config");
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

                using var configFile = new StreamWriter(TempConfigFilePath);
                configFile.Write(configString);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or SecurityException)
            {
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
                throw new InvalidOperationException("Must create or import config before committing");

            try
            {
                // Backup current config until save is complete
                if (Directory.Exists(BackupConfigFolderPath))
                    Directory.Delete(BackupConfigFolderPath, true);

                if (Directory.Exists(CurrentConfigFolderPath))
                    Directory.Move(CurrentConfigFolderPath, BackupConfigFolderPath);

                Directory.Move(TempConfigFolderPath, CurrentConfigFolderPath);
                _configCommitted = true;
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or SecurityException)
            {
                throw new ConfigIOException(e);
            }
            finally
            {
                // If not committed and a backup exists, move backup to become current.
                if (!_configCommitted && Directory.Exists(BackupConfigFolderPath))
                {
                    if (Directory.Exists(CurrentConfigFolderPath))
                        Directory.Delete(CurrentConfigFolderPath, true);

                    Directory.Move(BackupConfigFolderPath, CurrentConfigFolderPath);

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
        private static string ResolveFile(string file, string type, int id, int? i = null)
        {
            var extension = Path.GetExtension(file);

            var newFileName = i is null ? $"{type}_{id}{extension}" : $"{type}_{id}_{i}{extension}";
            string resolvedFilePath;
            string newFilePath;

            try
            {
                resolvedFilePath =
                    file.IsInConfig() ? Path.Combine(CurrentConfigFolderPath, file) : file;

                newFilePath = Path.Combine(TempConfigFolderPath, newFileName);
            }
            catch (ArgumentNullException e)
            {
                throw new ExternalFileReadException(file, e);
            }

            try
            {
                File.Copy(resolvedFilePath, newFilePath);
            }
            catch (FileNotFoundException e)
            {
                throw new ExternalFileReadException(file, e);
            }
            catch (IOException e)
            {
                if (File.Exists(newFilePath))
                    throw new ConfigDuplicateFileException(newFileName, e);

                throw new ConfigIOException(file, e);
            }

            return newFileName;
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
        private static IEnumerable<string> ResolveFiles(IEnumerable<string> files, string type, int id) =>
            files.Select((path, i) => ResolveFile(path, type, id, i));

        /// <summary>
        /// Delete temporary config folder and backup
        /// </summary>
        public void Dispose()
        {
            if (Directory.Exists(TempConfigFolderPath))
                Directory.Delete(TempConfigFolderPath, true);

            if (Directory.Exists(BackupConfigFolderPath))
                Directory.Delete(BackupConfigFolderPath, true);
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
