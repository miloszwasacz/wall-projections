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

        if (!File.Exists(hotspot.FullDescriptionPath))
        {
            throw new FileNotFoundException("No description file", hotspot.DescriptionPath);
        }

        var description = File.ReadAllText(hotspot.FullDescriptionPath);

        //TODO Refactor to support multiple images and videos
        var imagePath = hotspot.FullImagePaths.FirstOrDefault();
        var videoPath = hotspot.FullVideoPaths.FirstOrDefault();

        return new Hotspot.Media(hotspotId, description, imagePath, videoPath);
    }
}
