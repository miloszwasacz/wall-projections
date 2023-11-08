using System;
using System.IO;

namespace WallProjections.Models;

public static class FileLocator
{
    // Finds all the valid file names in the assets directory
    public static string[] GetFiles(string fileNumber)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Assets");
        var fileArray = Directory.GetFiles(path, fileNumber + ".*");
        return fileArray;
    }
}
