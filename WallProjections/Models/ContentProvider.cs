using System;
using System.Collections.Immutable;
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

        var fullDescriptionPath = hotspot.FullDescriptionPath;

        var description = File.ReadAllText(fullDescriptionPath);
        var imagePaths = hotspot.FullImagePaths;
        var videoPaths = hotspot.FullVideoPaths;

        return new Hotspot.Media(hotspotId, description, imagePaths, videoPaths);
    }
}
