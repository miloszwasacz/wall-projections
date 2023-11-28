using System;
using System.IO;

namespace WallProjections.Models.Interfaces;

public interface IContentCache : IDisposable
{
    /// <summary>
    /// Load a zip file of the config file and the media files
    /// </summary>
    /// <param name="zipPath">Path to the zip file</param>
    /// <returns>Config with the loaded</returns>
    /// <exception cref="FileNotFoundException">If zip file or config file could not be found</exception>
    public IConfig Load(string zipPath);

    /// <summary>
    /// Returns path to media files for a hotspot
    /// </summary>
    /// <param name="hotspot"><see cref="Hotspot" /> to get path for</param>
    /// <returns><i>string</i> with path to media files for hotspot</returns>
    public string GetHotspotMediaFolder(Hotspot hotspot);

    /// <summary>
    /// Create a new <see cref="IContentProvider" /> for the given <see cref="IConfig"/>.
    /// </summary>
    /// <param name="config">The <see cref="IConfig" /> containing data about the hotspots</param>
    /// <returns>A new instance of <see cref="IConfig" /></returns>
    public IContentProvider CreateContentProvider(IConfig config);
}
