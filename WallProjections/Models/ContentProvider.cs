using System.IO;
using System.Linq;
using WallProjections.Models.Interfaces;

namespace WallProjections.Models;

public class ContentProvider : IContentProvider
{
    private static readonly string[] ImageExtensions = { ".jpeg", ".JPEG", ".png", ".PNG", ".jpg", ".JPG" };
    private static readonly string[] VideoExtensions = { ".mp4", ".mov", ".mkv", ".mka", ".avi" };

    private readonly IContentCache _cache;
    private readonly IConfig _config;

    public ContentProvider(IContentCache cache, IConfig config)
    {
        _cache = cache;
        _config = config;
    }

    /// <inheritdoc />
    public Hotspot.Media GetMedia(int hotspotId)
    {
        var hotspot = _config.GetHotspot(hotspotId);
        if (hotspot is null)
            throw new IConfig.HotspotNotFoundException(hotspotId);

        var path = _cache.GetHotspotMediaFolder(hotspot);
        var files = Directory.GetFiles(path);

        var descriptionPath = files.FirstOrDefault(file => file.EndsWith(".txt"));
        if (descriptionPath is null)
            throw new FileNotFoundException("No .txt files found in hotspot folder");

        var imagePath = files.FirstOrDefault(file => ImageExtensions.Any(file.EndsWith));
        var videoPath = files.FirstOrDefault(file => VideoExtensions.Any(file.EndsWith));

        return new Hotspot.Media(hotspot, File.ReadAllText(descriptionPath), imagePath, videoPath);
    }
}
