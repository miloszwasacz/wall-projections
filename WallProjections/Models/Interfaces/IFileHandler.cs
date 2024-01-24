using System;
using System.IO;

namespace WallProjections.Models.Interfaces;

public interface IFileHandler : IDisposable
{
    /// <summary>
    /// Load a zip file of the config file and the media files
    /// </summary>
    /// <param name="zipPath">Path to the zip file</param>
    /// <returns>A config at the specified path, if any exist</returns>
    /// <exception cref="FileNotFoundException">If zip file or config file could not be found</exception>
    public IConfig? Load(string zipPath);

    /// <summary>
    /// Save the config file and the media files to a zip file
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public bool Save(IConfig config);
}
