using System;
using System.IO;

namespace WallProjections.Models.Interfaces;

public interface IFileHandler
{
    public const string ConfigFileName = "config.json";
    public const string ConfigFolderName = "WallProjections";

    public static string ConfigFolderPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ConfigFolderName);

    /// <summary>
    /// Import a zip file of the config file and the media files
    /// </summary>
    /// <param name="zipPath">Path to the zip file</param>
    /// <returns>A config at the specified path, if any exist</returns>
    /// <exception cref="FileNotFoundException">If zip file or config file could not be found</exception>
    public IConfig? Import(string zipPath);

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
    public bool Save(IConfig config);

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


    // TODO: Remove this
    public bool IsConfigImported();
}
