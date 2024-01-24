using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using WallProjections.Models.Interfaces;

namespace WallProjections.Models;

public sealed class FileHandler : IFileHandler
{
    private const string MediaLocation = "Media";
    private const string ConfigFileName = "config.json";

    /// <summary>
    /// The backing field for the <see cref="FileHandler" /> property
    /// </summary>
    private static FileHandler? _instance;

    /// <summary>
    /// A global instance of <see cref="FileHandler" />
    /// </summary>
    public static FileHandler Instance => _instance ??= new FileHandler();

    /// <summary>
    /// The backing field for <see cref="TempPath" />
    /// </summary>
    private string? _tempPath;

    private FileHandler()
    {
    }

    /// <summary>
    /// The path to the temporary folder where the zip file is extracted
    /// </summary>
    internal string TempPath => _tempPath ??= Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    /// <inheritdoc />
    /// <exception cref="JsonException">Format of config file is invalid</exception>
    /// TODO Handle errors from trying to load from not-found/invalid zip file
    public IConfig? Load(string zipPath)
    {
        // Clean up existing directly if in use
        if (Directory.Exists(TempPath))
            Directory.Delete(TempPath, true);

        Directory.CreateDirectory(TempPath);

        try
        {
            ZipFile.ExtractToDirectory(zipPath, TempPath);
            var config = LoadConfig(zipPath);
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
        throw new NotImplementedException();
    }

    /// <summary>
    /// Cleans up the temporary folder stored in <see cref="TempPath" />.
    /// </summary>
    public void Dispose()
    {
        try
        {
            Directory.Delete(TempPath, true);
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine($"{TempPath} already deleted");
        }
        catch (Exception e)
        {
            //TODO Write to log file instead
            Console.Error.WriteLine(e);
        }
    }

    /// <summary>
    /// Loads a config from a .json file
    /// </summary>
    /// <param name="zipPath">Path to zip containing config.json</param>
    /// <returns>Loaded Config</returns>
    /// <exception cref="JsonException">Format of config file is invalid</exception>
    /// <exception cref="FileNotFoundException">If config file cannot be found in zip file</exception>
    /// TODO More effective error handling of invalid/missing config files
    private static IConfig LoadConfig(string zipPath)
    {
        var zipFile = ZipFile.OpenRead(zipPath);
        var configEntry = zipFile.GetEntry(ConfigFileName);

        if (configEntry is null)
            throw new FileNotFoundException($"{ConfigFileName} not in root of zip file.");

        var config = JsonSerializer.Deserialize<Config>(configEntry.Open()) ?? throw new JsonException();
        return config;
    }
}
