using System.IO;
using System.Linq;
using Avalonia.Platform.Storage;

namespace WallProjections.Models.Interfaces;

public interface IContentProvider
{
    /// <summary>
    /// A list of supported video file extensions
    /// </summary>
    public static readonly string[] SupportedVideoExtensions = { ".mp4", ".mov", ".mkv", ".mka", ".avi" };

    /// <summary>
    /// A File Picker filter for all supported video files
    /// </summary>
    public static readonly FilePickerFileType FilePickerVideoType = new("All Videos")
    {
        Patterns = SupportedVideoExtensions.Select(ext => $"*{ext}").ToArray(),
        AppleUniformTypeIdentifiers = new[] { "public.movie" },
        MimeTypes = new[] { "video/*" }
    };

    /// <summary>
    /// Searches for the hotspot with the specified id and returns a <see cref="Hotspot.Media"/> object
    /// containing all the data and media paths associated with the hotspot
    /// </summary>
    /// <param name="hotspotId">The <see cref="Hotspot.Id"/> of a <see cref="Hotspot"/></param>
    /// <returns>A container with Hotspot's data</returns>
    /// <exception cref="IConfig.HotspotNotFoundException">
    /// The hotspot with the given <paramref name="hotspotId">ID</paramref> could not be found
    /// </exception>
    /// <exception cref="FileNotFoundException">The hotspot folder does not contain a description file</exception>
    public Hotspot.Media GetMedia(int hotspotId);
}
