using System;
using System.IO;

namespace WallProjections.Models.Interfaces;

public interface IContentImporter : IDisposable
{
    /// <summary>
    /// Load a zip file of the config file and the media files.
    /// </summary>
    /// <param name="zipPath">Path to the zip file</param>
    /// <returns>Config with the loaded</returns>
    /// <exception cref="FileNotFoundException">If zip file or config file could not be found</exception>
    public IConfig Load(string zipPath);

    /// <summary>
    /// Returns path to media files for a hotspot.
    /// </summary>
    /// <param name="hotspot"><see cref="Hotspot"/> to get path for.</param>
    /// <returns><i>string</i> with path to media files for hotspot.</returns>
    public string GetHotspotMediaFolder(Hotspot hotspot);
}
