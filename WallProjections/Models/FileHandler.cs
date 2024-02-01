using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using WallProjections.Models.Interfaces;

namespace WallProjections.Models;

public class FileHandler : IFileHandler
{
    private const string ConfigFileName = "config.json";
    private const string ConfigFolderName = "WallProjections";
    public static string ConfigFolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ConfigFolderName);

    public FileHandler()
    {
    }

    /// <inheritdoc />
    /// <exception cref="JsonException">Format of config file is invalid</exception>
    /// TODO Handle errors from trying to load from not-found/invalid zip file
    public IConfig? Load(string zipPath)
    {
        // Clean up existing directory if in use
        if (Directory.Exists(ConfigFolderPath))
            Directory.Delete(ConfigFolderPath, true);

        Directory.CreateDirectory(ConfigFolderPath);

        try
        {
            ZipFile.ExtractToDirectory(zipPath, ConfigFolderPath);
            Console.WriteLine("File extracted to {0}", ConfigFolderPath);
            var config = LoadConfig();
            return config;
        }
        catch (FileNotFoundException _)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public bool Save(IConfig config)
    {
        //TODO Implement saving
        var hotspots = config.Hotspots;
        var newHotspots = new List<Hotspot>();

        foreach (var hotspot in hotspots)
        {
            var newImagePaths = new List<string>(hotspot.ImagePaths);
            var newVideoPaths = new List<string>(hotspot.VideoPaths);



            var newDescriptionPath = $"text_{hotspot.Id}.txt";
            
            // Copy in non-imported description path.
            if (Path.IsPathRooted(hotspot.DescriptionPath))
            {
                File.Copy(hotspot.DescriptionPath, Path.Combine(ConfigFolderPath, newDescriptionPath));
            }
            else
            {
                File.Move(hotspot.DescriptionPath, Path.Combine(ConfigFolderPath, newDescriptionPath));   
            }   

            // Move already imported image files.
            newImagePaths = UpdateFiles(
                path => !Path.IsPathRooted(path),
                File.Move,
                newImagePaths,
                "image",
                hotspot.Id.ToString() );

            // Import non-imported image files.
            newImagePaths = UpdateFiles(
                Path.IsPathRooted,
                File.Copy,
                newImagePaths,
                "image",
                hotspot.Id.ToString() );

            // Move already imported video files.
            newVideoPaths = UpdateFiles(
                path => !Path.IsPathRooted(path),
                File.Move,
                newVideoPaths,
                "video",
                hotspot.Id.ToString() );

            // Import non-imported video files.
            newVideoPaths = UpdateFiles(
                Path.IsPathRooted,
                File.Copy,
                newVideoPaths,
                "video",
                hotspot.Id.ToString() );

            var newHotspot = new Hotspot(
                hotspot.Id,
                hotspot.Position,
                newDescriptionPath,
                newImagePaths.ToImmutableList(),
                newVideoPaths.ToImmutableList()
                );

            newHotspots.Add(newHotspot);

        }

        var newConfig = new Config(newHotspots);

        var serializerOptions = new JsonSerializerOptions();
        serializerOptions.WriteIndented = true;

        var configString = JsonSerializer.Serialize(newConfig, serializerOptions);

        var configFile = new StreamWriter(Path.Combine(ConfigFolderPath, ConfigFileName));
        configFile.Write(configString);
        configFile.Close();

        return true;
    }

    /// <summary>
    /// Moves and updates paths for files that match filter.
    /// </summary>
    /// <param name="filter">Decides if file will be processed.</param>
    /// <param name="fileUpdateFunc">The function run to update the location of the file.</param>
    /// <param name="oldPaths">All the old paths to be processed to new paths.</param>
    /// <param name="type">The type of file (image, video) to be updated.</param>
    /// <param name="id">The id of the hotspot for the new file name.</param>
    private static List<string> UpdateFiles (Predicate<string> filter, Action<string, string> fileUpdateFunc, IList<string> oldPaths, string type, string id)
    {
        var newPaths = new List<string>();

        for (var i = 0; i < oldPaths.Count; i++)
        {
            var oldPath = oldPaths[i];
            var newFileName = Path.GetFileName(oldPath);
            if (filter(oldPath))
            {
                var extension = Path.GetExtension(oldPath);
                newFileName = $"{type}_{id}_{i}.{extension}";

                fileUpdateFunc(oldPath,
                    Path.Combine(ConfigFolderPath, newFileName));
            }

            newPaths.Insert(i, newFileName);
        }

        return newPaths;
    }

    /// <summary>
    /// Cleans up the temporary folder stored in <see cref="ConfigFolderPath" />.
    /// </summary>
    public void Dispose()
    {
        try
        {
            Directory.Delete(ConfigFolderPath, true);
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine($"{ConfigFolderPath} already deleted");
        }
        catch (Exception e)
        {
            //TODO Write to log file instead
            Console.Error.WriteLine(e);
        }
    }

    /// <summary>
    /// Loads a config from the .json file imported/created in the program folder.
    /// </summary>
    /// <returns>Loaded Config</returns>
    /// <exception cref="JsonException">Format of config file is invalid</exception>
    /// <exception cref="FileNotFoundException">If config file cannot be found in zip file</exception>
    /// TODO More effective error handling of invalid/missing config files
    public static IConfig LoadConfig(string configLocation = null)
    {
        if (configLocation is null)
        {
            configLocation = Path.Combine(ConfigFolderPath, ConfigFileName);
        }

        FileStream configFile;
        try
        {
            configFile = File.OpenRead(configLocation);
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine(e);
            throw new FileNotFoundException("No config imported", configLocation);
        }

        var config = JsonSerializer.Deserialize<Config>(configFile)
                     ?? throw new JsonException("Config format invalid");
        
        configFile.Close();
        return config;
    }

    /// <summary>
    /// Checks if config has already been imported into the app folder.
    /// </summary>
    /// <returns>True if config imported, false otherwise.</returns>
    public static bool IsConfigImported()
    {
        return File.Exists(Path.Combine(ConfigFolderPath, ConfigFileName));
    }
}
