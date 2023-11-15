using System;
using System.IO;
using WallProjections.Models.Interfaces;

namespace WallProjections.Models;

public class FileProvider : IFileProvider
{
    //TODO Make this work as intended instead of using a hard-coded path
    /// <summary>
    /// Finds all the valid file names in the assets directory
    /// </summary>
    public string[] GetFiles(string fileNumber)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Assets");
        var fileArray = Directory.GetFiles(path, fileNumber + ".*");
        return fileArray;
    }
}
