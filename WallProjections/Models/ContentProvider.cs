using System.IO;
using System.Linq;
using WallProjections.Models.Interfaces;

namespace WallProjections.Models;

public class ContentProvider : IContentProvider
{
    private readonly IConfig _config;

    public ContentProvider(IConfig config)
    {
        _config = config;
    }

    /// <inheritdoc />
    public Hotspot.Media GetMedia(int hotspotId)
    {
        var hotspot = _config.GetHotspot(hotspotId);
        if (hotspot is null)
            throw new IConfig.HotspotNotFoundException(hotspotId);

        // TODO Include the path to the config so that the media files can use relative paths
        // (need to update returned image/video paths)
        var description = File.ReadAllText(Path.Combine(FileHandler.ConfigFolderPath, hotspot.DescriptionPath));
        //TODO Refactor to support multiple images and videos
        var imagePath = hotspot.ImagePaths.FirstOrDefault();
        var videoPath = hotspot.VideoPaths.FirstOrDefault();

        return new Hotspot.Media(description, imagePath, videoPath);
    }
}
