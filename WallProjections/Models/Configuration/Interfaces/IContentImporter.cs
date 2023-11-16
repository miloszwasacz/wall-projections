using System.Text.Json;

namespace WallProjections.Models.Configuration.Interfaces;

public interface IContentImporter
{
    /// <summary>
    /// Location where the opened files are stored.
    /// </summary>
    public string TempPath { get; }

    /// <summary>
    /// Load a zip file of the config.json file and the media files.
    /// </summary>
    /// <param name="zipPath">Path to the zip file</param>
    /// <returns>Config with the loaded</returns>
    public IConfig Load(string zipPath);

    /// <summary>
    /// Cleans up the temporary folder stored in <see cref="TempPath"/>.
    /// </summary>
    public void Cleanup();

    /// <summary>
    /// Loads a config from a .json file.
    /// </summary>
    /// <param name="zipPath">Path to zip containing config.json.</param>
    /// <returns>Loaded Config.</returns>
    /// <exception cref="JsonException">Format of config file is invalid.</exception>
    public IConfig LoadConfig(string zipPath);

    /// <summary>
    /// Returns path to files for a hotspot.
    /// </summary>
    /// <param name="hotspot"><see cref="Hotspot"/> to get path for.</param>
    /// <returns><see cref="string"/> with path to media files for hotspot.</returns>
    public string GetHotspotFolder(Hotspot hotspot);
}
