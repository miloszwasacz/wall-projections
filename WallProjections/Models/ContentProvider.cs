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
        
        var description = "";

        if (File.Exists(hotspot.FullDescriptionPath))
        {
            description = File.ReadAllText(hotspot.FullDescriptionPath);
        }
        
        //TODO Refactor to support multiple images and videos
        var imagePath = hotspot.FullImagePaths.FirstOrDefault();
        var videoPath = hotspot.FullVideoPaths.FirstOrDefault();

        return new Hotspot.Media(description, imagePath, videoPath);
    }
}
