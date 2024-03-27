using System;
using System.IO;

namespace WallProjections.Models.Interfaces;

public interface IFileHandler
{
    public const string ConfigFileName = "config.json";
    public const string ConfigFolderName = "WallProjections";

    /// <summary>
    /// Path to the folder containing the config.json and the media files.
    /// </summary>
    public static string ConfigFolderPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create ), ConfigFolderName);

    /// <summary>
    /// Import a zip file of the config file and the media files
    /// </summary>
    /// <param name="zipPath">Path to the zip file</param>
    /// <returns>A config at the specified path, if any exist</returns>
    /// <exception cref="FileNotFoundException">If zip file or config file could not be found</exception>
    public IConfig? ImportConfig(string zipPath);

    /// <summary>
    /// Export config file and media file to a zip file.
    /// </summary>
    /// <param name="exportPath">Path to the zip file</param>
    /// <returns>True if file is exported</returns>
    /// <exception cref="DirectoryNotFoundException">If there is no imported config/media.</exception>
    public bool ExportConfig(string exportPath);

    /// <summary>
    /// Loads the config.json file from the config folder.
    /// </summary>
    /// <returns>Loaded <see cref="IConfig"/></returns>
    public IConfig LoadConfig();

    /// <summary>
    /// Save the config file and the media files to a zip file. Any media files that point to an absolute path
    /// will be imported and the paths renamed to the new path automatically.
    /// </summary>
    /// <param name="config">The new config file to save</param>
    /// <returns>True if saved successfully</returns>
    public bool SaveConfig(IConfig config);

    /// <summary>
    /// Removes all files and the folder<see cref="ConfigFolderPath" />.
    /// </summary>
    public static void DeleteConfigFolder()
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
}
