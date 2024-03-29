using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using WallProjections.Models.Interfaces;
using static WallProjections.Models.Interfaces.IFileHandler;

namespace WallProjections.Models;

public class FileHandler : IFileHandler
{
    /// <inheritdoc />
    /// <exception cref="ConfigException">Format of config file is invalid</exception>
    /// TODO Handle errors from trying to load from not-found/invalid zip file
    public IConfig ImportConfig(string zipPath)
    {
        // Clean up existing directory if in use
        if (Directory.Exists(CurrentConfigFolderPath))
            Directory.Delete(CurrentConfigFolderPath, true);

        Directory.CreateDirectory(CurrentConfigFolderPath);

        ZipFile.ExtractToDirectory(zipPath, CurrentConfigFolderPath);

        var config = LoadConfig();
        return config;
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
        // Check if directory already exists. If not, create it.
        if (!Directory.Exists(CurrentConfigFolderPath))
            Directory.CreateDirectory(CurrentConfigFolderPath);

        var hotspots = config.Hotspots.Select(hotspot =>
        {
            var newDescriptionPath = $"text_{hotspot.Id}.txt";

            // Copy in non-imported description path.
            if (!hotspot.DescriptionPath.IsInConfig())
            {
                File.Copy(
                    hotspot.DescriptionPath,
                    Path.Combine(CurrentConfigFolderPath, newDescriptionPath),
                    true
                );
            }
            else
            {
                File.Move(
                    Path.Combine(CurrentConfigFolderPath, hotspot.DescriptionPath),
                    Path.Combine(CurrentConfigFolderPath, newDescriptionPath),
                    true
                );
            }

            var newImagePaths = UpdateFiles( // Move already imported image files.
                PathExtensions.IsInConfig,
                File.Move,
                hotspot.ImagePaths,
                "image",
                hotspot.Id
            ).Concat(UpdateFiles( // Import non-imported image files.
                PathExtensions.IsNotInConfig,
                File.Copy,
                hotspot.ImagePaths,
                "image",
                hotspot.Id
            )).ToImmutableList();

            var newVideoPaths = UpdateFiles( // Move already imported video files.
                PathExtensions.IsInConfig,
                File.Move,
                hotspot.VideoPaths,
                "video",
                hotspot.Id
            ).Concat(UpdateFiles( // Import non-imported video files.
                PathExtensions.IsNotInConfig,
                File.Copy,
                hotspot.VideoPaths,
                "video",
                hotspot.Id
            )).ToImmutableList();

            return new Hotspot(
                hotspot.Id,
                hotspot.Position,
                hotspot.Title,
                newDescriptionPath,
                newImagePaths,
                newVideoPaths
            );
        });

        var newConfig = new Config(config.HomographyMatrix, hotspots);

#if RELEASE
        var configString = JsonSerializer.Serialize(newConfig);
#else
        var configString = JsonSerializer.Serialize(newConfig, new JsonSerializerOptions
        {
            WriteIndented = true
        });
#endif

        using var configFile = new StreamWriter(Path.Combine(CurrentConfigFolderPath, ConfigFileName));
        configFile.Write(configString);

        return true;
    }

    /// <summary>
    /// Moves and updates paths for files that match filter.
    /// </summary>
    /// <param name="filter">Decides if file will be processed.</param>
    /// <param name="fileUpdateFunc">The function run to update the location of the file.</param>
    /// <param name="filePaths">All the old paths to be processed to new paths.</param>
    /// <param name="type">The type of file (image, video) to be updated.</param>
    /// <param name="id">The id of the hotspot for the new file name.</param>
    private static IEnumerable<string> UpdateFiles(
        Func<string, bool> filter,
        Action<string, string, bool> fileUpdateFunc,
        IEnumerable<string> filePaths,
        string type,
        int id
    ) => filePaths
        .Where(filter)
        .Select((path, i) =>
        {
            var extension = Path.GetExtension(path);
            var newFileName = $"{type}_{id}_{i}{extension}";

            fileUpdateFunc(path, Path.Combine(CurrentConfigFolderPath, newFileName), true);

            return newFileName;
        });

    /// <summary>
    /// Loads a config from the .json file imported/created in the program folder.
    /// </summary>
    /// <returns>Loaded Config</returns>
    /// <exception cref="JsonException">Format of config file is invalid</exception>
    /// <exception cref="FileNotFoundException">If config file cannot be found in zip file</exception>
    /// TODO More effective error handling of invalid/missing config files
    public IConfig LoadConfig()
    {
        var configLocation = Path.Combine(CurrentConfigFolderPath, ConfigFileName);

        using var configFile = File.OpenRead(configLocation);
        return JsonSerializer.Deserialize<Config>(configFile) ??
               throw new JsonException("Config format invalid");
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
