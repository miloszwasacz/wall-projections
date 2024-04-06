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

public class FileHandler : IFileHandler
{
    /// <inheritdoc />
    /// <exception cref="ExternalFileReadException">Could not read </exception>
    /// <exception cref="ConfigException">Format of config file is invalid</exception>
    /// TODO Handle errors from trying to load from not-found/invalid zip file
    public IConfig ImportConfig(string zipPath)
    {
        using var configBuilder = new ConfigBuilder();

        configBuilder.ImportConfig(zipPath);

        configBuilder.Commit();

        return LoadConfig();
    }

    /// <inheritdoc/>
    public bool ExportConfig(string exportPath)
    {
        if (!Directory.Exists(CurrentConfigFolderPath))
            throw new ConfigNotImportedException();

        ZipFile.CreateFromDirectory(CurrentConfigFolderPath, exportPath);
        return true;
    }


    /// <inheritdoc />
    /// <exception>
    /// Can throw the same exceptions as <see cref="Directory.CreateDirectory" />,
    /// <see cref="File.Copy(string, string)" />, and <see cref="File.Move(string, string, bool)" />.
    /// </exception>
    public bool SaveConfig(IConfig config)
    {
        using var configBuilder = new ConfigBuilder();

        configBuilder.AddHomographyMatrix(config.HomographyMatrix);

        config.Hotspots.ForEach(hotspot => { configBuilder.AddHotspot(hotspot); });

        configBuilder.GenerateConfigFile();

        configBuilder.Commit();

        return true;
    }

    /// <summary>
    /// Loads a config from the .json file imported/created in the program folder.
    /// </summary>
    /// <returns>Loaded Config</returns>
    /// <exception cref="ConfigNotImportedException">If <see cref="LoadConfig"/> called when no config imported.</exception>
    /// <exception cref="ConfigInvalidException">config.json missing or has invalid syntax.</exception>
    public IConfig LoadConfig()
    {
        var configLocation = Path.Combine(CurrentConfigFolderPath, ConfigFileName);

        if (!Directory.Exists(CurrentConfigFolderPath))
            throw new ConfigNotImportedException();

        if (!File.Exists(configLocation))
            throw new ConfigInvalidException();

        try
        {
            using var configFile = File.OpenRead(configLocation);
            return JsonSerializer.Deserialize<Config>(configFile) ??
                   throw new ConfigInvalidException();
        }
        catch (IOException)
        {
            throw new ConfigIOException();
        }
        // When JSON config is not in a valid format.
        catch (Exception e) when (e is JsonException or ArgumentNullException)
        {
            throw new ConfigInvalidException();
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
        private double[,] _homographyMatrix;

        /// <summary>
        /// List of hotspots to be stored in the new config.
        /// </summary>
        private List<Hotspot> _hotspots;

        /// <summary>
        /// Store if a new config will be generated
        /// </summary>
        private bool _newConfig;

        /// <summary>
        /// Store if config file has been generated
        /// </summary>
        private bool _generatedConfig;

        /// <summary>
        /// Store if an imported config will be used
        /// </summary>
        private bool _importedConfig;

        /// <summary>
        /// Stores whether the new config has been committed to storage.
        /// </summary>
        private bool _committed;

        public ConfigBuilder()
        {
            // Reset/Create temporary config folder
            if (Directory.Exists(TempConfigFolderPath))
                Directory.Delete(TempConfigFolderPath);

            Directory.CreateDirectory(TempConfigFolderPath);

            _hotspots = new List<Hotspot>();

            _committed = false;
            _importedConfig = false;
            _newConfig = false;
            _generatedConfig = false;
        }

        /// <summary>
        /// Import
        /// </summary>
        /// <param name="zipPath"></param>
        /// <exception cref="ConfigBuilderInvalidException">If a new config has already started being built</exception>
        /// <exception cref="ExternalFileReadException">If the package does not exist</exception>
        /// <exception cref="ConfigIOException">If there is an issue accessing package/config folder</exception>
        /// <exception cref="ConfigPackageFormatException">If config package is not a valid package</exception>
        public void ImportConfig(string zipPath)
        {
            if (_newConfig)
                throw new ConfigBuilderInvalidException();
            _importedConfig = true;

            try
            {
                // Check if zip file exists for import
                if (!File.Exists(zipPath))
                    throw new ExternalFileReadException(Path.GetFileName(zipPath));

                ZipFile.ExtractToDirectory(zipPath, TempConfigFolderPath);

            }
            catch (IOException e)
            {
                throw new ConfigIOException();
            }
            catch (InvalidDataException e)
            {
                throw new ConfigPackageFormatException(Path.GetFileName(zipPath));
            }
            catch (UnauthorizedAccessException e)
            {
                throw new ConfigIOException();
            }
        }

        /// <summary>
        /// Add the homography matrix to be stored inside the config
        /// </summary>
        /// <param name="homographyMatrix">The homography matrix to be stored in the config</param>
        public void AddHomographyMatrix(double[,] homographyMatrix)
        {
            if (_importedConfig)
                throw new ConfigBuilderInvalidException();

            if (_homographyMatrix is not null)
                throw new ConfigBuilderInvalidException("Homography matrix already added");

            _newConfig = true;

            _homographyMatrix = homographyMatrix;
        }

        /// <summary>
        /// Adds a hotspot to the new config.
        /// </summary>
        /// <param name="hotspot">Hotspot to resolve/add to new config.</param>
        /// <exception cref="ConfigDuplicateFileException">If a file with the same resolved name is already imported.</exception>
        /// <exception cref="ExternalFileReadException">If one of the files to resolve cannot be read.</exception>
        public void AddHotspot(Hotspot hotspot)
        {
            if (_importedConfig)
                throw new ConfigBuilderInvalidException();

            if (_generatedConfig)
                throw new ConfigBuilderInvalidException("Config file already generated");

            _newConfig = true;

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
        public void GenerateConfigFile()
        {
            if (_importedConfig)
                throw new ConfigBuilderInvalidException("Cannot generate config file for imported config");

            if (_homographyMatrix is null)
                throw new ConfigBuilderInvalidException("Must have homography matrix");

            if (_generatedConfig)
                throw new ConfigBuilderInvalidException("Config file already generated for config");
            _generatedConfig = true;

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

                using var configFile = new StreamWriter(Path.Combine(TempConfigFolderPath, ConfigFileName));
                configFile.Write(configString);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or SecurityException)
            {
                throw new ConfigIOException();
            }
        }

        /// <summary>
        /// Commits the new config and overwrites the old config.
        /// </summary>
        /// <exception cref="ConfigIOException">If a permissions/IO error occurs during the commit</exception>
        public void Commit()
        {
            if (!_newConfig && !_importedConfig)
                throw new ConfigBuilderInvalidException("Must have imported or built new config");

            if (_newConfig && !_generatedConfig)
                throw new ConfigBuilderInvalidException("Must generate config file for new config before commit");

            try
            {
                // Backup current config until save is complete
                if (Directory.Exists(BackupConfigFolderPath))
                    Directory.Delete(BackupConfigFolderPath, true);

                if (Directory.Exists(CurrentConfigFolderPath))
                    Directory.Move(CurrentConfigFolderPath, BackupConfigFolderPath);

                Directory.Move(TempConfigFolderPath, CurrentConfigFolderPath);
                _committed = true;
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or SecurityException)
            {
                Console.WriteLine(e);
                throw new ConfigIOException();
            }
            finally
            {
                // If not committed and a backup exists, move backup to become current.
                if (!_committed && Directory.Exists(BackupConfigFolderPath))
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
        private static string ResolveFile(string file, string type, int id, int? i = null)
        {
            var currentFileName = Path.GetFileName(file);

            if (currentFileName is null or "")
                throw new ExternalFileReadException(file);

            var extension = Path.GetExtension(file);

            if (extension is null or "")
                throw new ExternalFileReadException(currentFileName);

            var newFileName = i is null ? $"{type}_{id}{extension}" : $"{type}_{id}_{i}{extension}";

            var resolvedFilePath =
                file.IsNotInConfig() ? file : Path.Combine(CurrentConfigFolderPath, file);

            var newFilePath = Path.Combine(TempConfigFolderPath, newFileName);

            if (File.Exists(newFilePath))
                throw new ConfigDuplicateFileException(newFileName);

            if (!File.Exists(resolvedFilePath))
                throw new ExternalFileReadException(Path.GetFileName(resolvedFilePath));

            try
            {
                File.Copy(resolvedFilePath, newFilePath);
            }
            catch (IOException e)
            {
                throw new ConfigIOException();
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
        /// <exception cref="ConfigDuplicateFileException">If a file with the same resolved name is already imported.</exception>
        /// <exception cref="ExternalFileReadException">If one of the files to resolve cannot be read.</exception>
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
    public static bool IsInConfig(this string path) => path.StartsWith(CurrentConfigFolderPath) && File.Exists(path);

    /// <summary>
    /// Checks if the file is not in the <see cref="IFileHandler.CurrentConfigFolderPath">config folder</see>.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    /// <seealso cref="IsInConfig" />
    public static bool IsNotInConfig(this string path) => !path.IsInConfig() && File.Exists(path);
}
