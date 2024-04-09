using System;
using System.IO;

namespace WallProjections.Models.Interfaces;

public interface IFileHandler
{
    private const string ConfigFileName = "config.json";
    private const string ConfigFolderName = "WallProjections";
    private const string CurrentConfigFolder = "Current";
    private const string TempConfigFolder = "Temp";
    private const string BackupConfigFolder = "Backup";

    /// <summary>
    /// Path to the config folder for the program.
    /// </summary>
    public static string AppDataFolderPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData,
                Environment.SpecialFolderOption.Create),
            ConfigFolderName);

    /// <summary>
    /// Path to the folder containing the current config.json and the media files.
    /// </summary>
    public static string CurrentConfigFolderPath => Path.Combine(AppDataFolderPath, CurrentConfigFolder);

    /// <summary>
    /// Path to the current config.json file.
    /// </summary>
    public static string CurrentConfigFilePath => Path.Combine(CurrentConfigFolderPath, ConfigFileName);

    /// <summary>
    /// Path to the folder used to store the temporary config during saving.
    /// </summary>
    public static string TempConfigFolderPath => Path.Combine(AppDataFolderPath, TempConfigFolder);

    /// <summary>
    /// Path to the config.json file during saving.
    /// </summary>
    public static string TempConfigFilePath => Path.Combine(TempConfigFolderPath, ConfigFileName);

    /// <summary>
    /// Path to the folder for storing the current config while the new config is being saved.
    /// </summary>
    public static string BackupConfigFolderPath => Path.Combine(AppDataFolderPath, BackupConfigFolder);

    /// <summary>
    /// Import a zip file of the config file and the media files
    /// </summary>
    /// <param name="zipPath">Path to the zip file</param>
    /// <returns>A config at the specified path, if any exist</returns>
    /// <exception cref="ExternalFileReadException">Could not access config package.</exception>
    /// <exception cref="ConfigInvalidException">Format of config file is invalid.</exception>
    /// <exception cref="ConfigPackageFormatException">If format of config package is invalid.</exception>
    public IConfig? ImportConfig(string zipPath);

    /// <summary>
    /// Export config file and media file to a zip file.
    /// </summary>
    /// <param name="exportPath">Path to the zip file</param>
    /// <returns>True if file is exported</returns>
    /// <exception cref="ConfigNotImportedException">If there is no imported config to export.</exception>
    /// <exception cref="ConfigDuplicateFileException"> If file already exists at <see cref="exportPath"/>.</exception>
    /// <exception cref="ConfigIOException">If there is an issue saving the package file or accessing config.</exception>
    public bool ExportConfig(string exportPath);

    /// <summary>
    /// Loads the config.json file from the config folder.
    /// </summary>
    /// <returns>Loaded <see cref="IConfig"/></returns>
    /// <exception cref="ConfigInvalidException">If the config.json file is not a valid <see cref="IConfig"/></exception>
    /// <exception cref="ConfigIOException">If there is an issue accessing internal config files/folders.</exception>
    public IConfig LoadConfig();

    /// <summary>
    /// Save the config file and the media files to a zip file. Any media files that point to an absolute path
    /// will be imported and the paths renamed to the new path automatically.
    /// </summary>
    /// <param name="config">The new config file to save</param>
    /// <returns>True if saved successfully</returns>
    /// <exception cref="ExternalFileReadException">If one of the external files cannot be read.</exception>
    /// <exception cref="ConfigDuplicateFileException">If two files with the same name are added to the config.</exception>
    /// <exception cref="ConfigIOException">If there is an issue accessing media files/saving the config to disk.</exception>
    public bool SaveConfig(IConfig config);

    /// <summary>
    /// Removes all files and the folder <see cref="CurrentConfigFolderPath" />.
    /// </summary>
    /// <exception cref="ConfigNotImportedException">If there is no imported config to delete.</exception>
    /// <exception cref="ConfigIOException">If config folder is locked or inaccessible.</exception>
    public static void DeleteConfigFolder()
    {
        try
        {
            Directory.Delete(CurrentConfigFolderPath, true);
        }
        catch (DirectoryNotFoundException e)
        {
            throw new ConfigNotImportedException(e);
        }
        catch (Exception e) when (e is UnauthorizedAccessException or IOException)
        {
            throw new ConfigIOException(e);
        }
    }
}
