using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using WallProjections.Models.Interfaces;

namespace WallProjections.Models;

public class ContentProvider : IContentProvider
{
    private readonly IConfig _config;
    private readonly string _folderPath;

    public ContentProvider(IConfig config, string folderPath = "")
    {
        _config = config;
        _folderPath = folderPath;
    }

    /// <inheritdoc />
    public Hotspot.Media GetMedia(int hotspotId)
    {
        var hotspot = _config.GetHotspot(hotspotId);
        if (hotspot is null)
            throw new IConfig.HotspotNotFoundException(hotspotId);

        var fullDescriptionPath = _folderPath == "" ? hotspot.FullDescriptionPath
            : Path.Combine(_folderPath, hotspot.DescriptionPath) ;

        if (!File.Exists(fullDescriptionPath))
        {
            throw new FileNotFoundException($"No description file at {fullDescriptionPath}", hotspot.DescriptionPath);
        }

        var description = File.ReadAllText(fullDescriptionPath);

        //TODO Refactor to support multiple images and videos
        ImmutableList<string> imagePaths = _folderPath == "" ? hotspot.FullImagePaths
                : hotspot.ImagePaths.ConvertAll(path => Path.Combine(_folderPath, path));

        ImmutableList<string> videoPaths = _folderPath == "" ? hotspot.FullVideoPaths
            : hotspot.VideoPaths.ConvertAll(path => Path.Combine(_folderPath, path));

        return new Hotspot.Media(hotspotId, description, imagePaths, videoPaths);
    }
}
