using System;
using System.IO;
using System.IO.Compression;

namespace WallProjections.Configuration;

public class ContentImporter
{
    public static Config Load(string zipPath)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        while (Directory.Exists(tempPath))
        {
            tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }

        var folderInfo = Directory.CreateDirectory(tempPath);

        ZipFile.ExtractToDirectory(zipPath, tempPath);

        var config = Config.LoadConfig(tempPath, "config.json");
        return config;
    }

    /// <summary>
    /// Cleans up the temporary folder.
    /// </summary>
    /// <param name="config">The Config class used to access the files.</param>
    public static void Cleanup(Config config)
    {
        Directory.Delete(config.TempPath);
    }
}
