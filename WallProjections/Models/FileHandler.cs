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
    /// <exception cref="JsonException">Format of config file is invalid</exception>
    /// TODO Handle errors from trying to load from not-found/invalid zip file
    public IConfig ImportConfig(string zipPath)
    {
        // Clean up existing directory if in use
        if (Directory.Exists(ConfigFolderPath))
            Directory.Delete(ConfigFolderPath, true);

        Directory.CreateDirectory(ConfigFolderPath);

        ZipFile.ExtractToDirectory(zipPath, ConfigFolderPath);
        Console.WriteLine("File extracted to {0}", ConfigFolderPath);
        var config = LoadConfig();
        return config;
    }

    /// <summary>
    /// Exports the config files to a zip file with specified path.
    /// </summary>
    /// <param name="zipPath">Path to export zip to.</param>
    /// <returns>true if export is successful.</returns>
    public bool ExportConfig(string zipPath)
    {
        if (!Directory.Exists(ConfigFolderPath))
            throw new DirectoryNotFoundException("No imported/created config to export.");
        ZipFile.CreateFromDirectory(ConfigFolderPath, zipPath);
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
        if (!Directory.Exists(ConfigFolderPath))
            Directory.CreateDirectory(ConfigFolderPath);

        var hotspots = config.Hotspots;
        var newHotspots = new List<Hotspot>();

        foreach (var hotspot in hotspots)
        {
            var newDescriptionPath = $"text_{hotspot.Id}.txt";

            // Copy in non-imported description path.
            if (!hotspot.DescriptionPath.IsInConfig())
            {
                File.Copy(
                    hotspot.DescriptionPath,
                    Path.Combine(ConfigFolderPath, newDescriptionPath),
                    true
                );
            }
            else
            {
                File.Move(
                    Path.Combine(ConfigFolderPath, hotspot.DescriptionPath),
                    Path.Combine(ConfigFolderPath, newDescriptionPath),
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

            var newHotspot = new Hotspot(
                hotspot.Id,
                hotspot.Position,
                hotspot.Title,
                newDescriptionPath,
                newImagePaths,
                newVideoPaths
            );

            newHotspots.Add(newHotspot);
        }

        var newConfig = new Config(newHotspots);

#if DEBUG || DEBUGSKIPPYTHON
        var configString = JsonSerializer.Serialize(newConfig, new JsonSerializerOptions
        {
            WriteIndented = true
        });
#else
        var configString = JsonSerializer.Serialize(newConfig);
#endif

        using var configFile = new StreamWriter(Path.Combine(ConfigFolderPath, ConfigFileName));
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

            fileUpdateFunc(path, Path.Combine(ConfigFolderPath, newFileName), true);

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
        var configLocation = Path.Combine(ConfigFolderPath, ConfigFileName);

        using var configFile = File.OpenRead(configLocation);
        return JsonSerializer.Deserialize<Config>(configFile) ??
               throw new JsonException("Config format invalid");
    }

    /// <summary>
    /// Checks if config has already been imported into the app folder.
    /// </summary>
    /// <returns>True if config imported, false otherwise.</returns>
    public bool IsConfigImported()
    {
        return File.Exists(Path.Combine(ConfigFolderPath, ConfigFileName));
    }
}

/// <summary>
/// Extension methods for media file paths.
/// </summary>
internal static class PathExtensions
{
    /// <summary>
    /// Checks if the file is in the <see cref="IFileHandler.ConfigFolderPath">config folder</see>.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    /// <seealso cref="IsNotInConfig" />
    public static bool IsInConfig(this string path) => path.StartsWith(ConfigFolderPath) && File.Exists(path);

    /// <summary>
    /// Checks if the file is not in the <see cref="IFileHandler.ConfigFolderPath">config folder</see>.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    /// <seealso cref="IsInConfig" />
    public static bool IsNotInConfig(this string path) => !path.IsInConfig() && File.Exists(path);
}
