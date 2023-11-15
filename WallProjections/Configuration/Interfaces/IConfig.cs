using System.IO;

namespace WallProjections.Configuration.Interfaces;

public interface IConfig
{
    /// <summary>
    /// Backing field for <see cref="TempPath"/>
    /// </summary>
    private static string? _tempPath;

    /// <summary>
    /// Location where the opened files are stored.
    /// </summary>
    public static string TempPath => _tempPath ??= Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    /// <summary>
    /// Location where to store configuration.
    /// </summary>
    public string ConfigLocation { get; }

    /// <summary>
    /// Returns hotspot if Id matches a hotspot.
    /// </summary>
    /// <param name="id">Id to match Hotspot</param>
    /// <returns><see cref="Hotspot"/> with matching Id if exists, or null if no such hotspot.</returns>
    public Hotspot? GetHotspot(int id);

    /// <summary>
    /// Number of hotspots stored in <see cref="Config"/>.
    /// </summary>
    /// <returns><see cref="int"/> of count of hotspots in config.</returns>
    public int HotspotCount();

    /// <summary>
    /// Returns path to files for a hotspot.
    /// </summary>
    /// <param name="hotspot"><see cref="Hotspot"/> to get path for.</param>
    /// <returns><see cref="string"/> with path to media files for hotspot.</returns>
    public static string GetHotspotFolder(Hotspot hotspot)
    {
        return Path.Combine(TempPath, hotspot.Id.ToString());
    }
}
