using System.IO;
using System.Text.Json;

namespace WallProjections.Models.Configuration.Interfaces;

public interface IContentImporter
{
    /// <summary>
    /// Load a zip file of the config.json file and the media files.
    /// </summary>
    /// <param name="zipPath">Path to the zip file</param>
    /// <returns>Config with the loaded</returns>
    /// <exception cref="FileNotFoundException">
    ///     If zip file or config file could not be found.
    /// </exception>
    /// TODO: Handle errors from trying to load from not-found/invalid zip file.
    public IConfig Load(string zipPath);

    /// <summary>
    /// Cleans up the temporary folder stored in <see cref="TempPath"/>.
    /// </summary>
    public void Cleanup();

    /// <summary>
    /// Returns path to media files for a hotspot.
    /// </summary>
    /// <param name="hotspot"><see cref="Hotspot"/> to get path for.</param>
    /// <returns><see cref="string"/> with path to media files for hotspot.</returns>
    public string GetHotspotMediaFolder(Hotspot hotspot);
}
