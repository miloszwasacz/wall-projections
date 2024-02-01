using System;
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

        Console.WriteLine($"Full description path {fullDescriptionPath}");
        var description = File.ReadAllText(fullDescriptionPath);

        //TODO Refactor to support multiple images and videos
        string imagePath = null;
        string videoPath = null;

        if (!hotspot.ImagePaths.IsEmpty)
        {
            imagePath = _folderPath == "" ? hotspot.FullImagePaths.FirstOrDefault()
                : Path.Combine(_folderPath, hotspot.ImagePaths.FirstOrDefault()) ;
        }

        if (!hotspot.VideoPaths.IsEmpty)
        {
            videoPath = _folderPath == "" ? hotspot.FullVideoPaths.FirstOrDefault()
                : Path.Combine(_folderPath, hotspot.VideoPaths.FirstOrDefault());
        }

        return new Hotspot.Media(hotspotId, description, imagePath, videoPath);
    }
}
