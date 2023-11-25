using WallProjections.Models;
using WallProjections.Models.Interfaces;

namespace WallProjections.Test.Mocks.Models;

public sealed class MockContentCache : IContentCache
{
    private readonly List<string> _loadedZips = new();

    private readonly List<Hotspot.Media> _media;

    public IConfig Config { get; set; }

    public IReadOnlyList<string> LoadedZips => _loadedZips;

    public string MediaPath { get; set; }

    public int LoadCount => _loadedZips.Count;

    public int DisposeCount { get; private set; }

    public MockContentCache(IConfig config, string mediaPath, List<Hotspot.Media> files)
    {
        _media = files;
        MediaPath = mediaPath;
        Config = config;
    }

    public IConfig Load(string zipPath)
    {
        _loadedZips.Add(zipPath);
        return Config;
    }

    public string GetHotspotMediaFolder(Hotspot hotspot)
    {
        return Path.Combine(MediaPath, hotspot.Id.ToString());
    }

    public IContentProvider CreateContentProvider(IConfig config) => new MockContentProvider(_media);

    public void Dispose()
    {
        DisposeCount++;
    }
}
