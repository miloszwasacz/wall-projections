using System.IO;
using WallProjections.Models.Interfaces;

namespace WallProjections.Models;

public class ContentProvider : IContentProvider
{
    /// <summary>
    /// The <see cref="IConfig"/> for fetching data about the artifact.
    /// </summary>
    private readonly IConfig _config;

    /// <summary>
    /// Create a new <see cref="ContentProvider"/> with the given <see cref="IConfig"/>.
    /// </summary>
    /// <param name="config">The <see cref="IConfig"/> for fetching data obout the artifact.</param>
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

        var title = hotspot.Title;
        var description = File.ReadAllText(fullDescriptionPath);
        var imagePaths = hotspot.FullImagePaths;
        var videoPaths = hotspot.FullVideoPaths;

        return new Hotspot.Media(hotspotId, title, description, imagePaths, videoPaths);
    }
}
