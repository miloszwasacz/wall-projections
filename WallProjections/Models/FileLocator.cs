using System;
using System.IO;

namespace WallProjections.Models;

public static class FileLocator
{
    //finds all the valid file names in the assets directory
    public static string[] GetFiles(string fileNumber)
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)+"/Assets/";
        string[] fileArray = Directory.GetFiles(path, fileNumber + ".*");
        return fileArray;
    }
} 
