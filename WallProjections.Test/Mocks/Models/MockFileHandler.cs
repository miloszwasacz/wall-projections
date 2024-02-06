using WallProjections.Models;
using WallProjections.Models.Interfaces;

namespace WallProjections.Test.Mocks.Models;

public sealed class MockFileHandler : IFileHandler
{
    /// <summary>
    /// The backing field for <see cref="LoadedZips" />
    /// </summary>
    private readonly List<string> _loadedZips = new();

    /// <summary>
    /// The media list passed to the <see cref="MockContentProvider" />'s constructor
    /// when <see cref="CreateContentProvider" /> is called
    /// </summary>
    private readonly List<Hotspot.Media> _media;

    /// <summary>
    /// The exception passed to the <see cref="MockContentProvider" />'s constructor
    /// when <see cref="CreateContentProvider" /> is called
    /// </summary>
    private readonly Exception? _exception;

    /// <summary>
    /// The <see cref="IConfig" /> to be returned by <see cref="Import" />
    /// </summary>
    public IConfig Config { get; set; }

    /// <summary>
    /// A list of all the zip files that have been loaded using <see cref="Import" />
    /// </summary>
    public IReadOnlyList<string> LoadedZips => _loadedZips;

    /// <summary>
    /// The theoretical path to the folder containing the media files
    /// </summary>
    public string MediaPath { get; set; }

    /// <summary>
    /// The number of times <see cref="Import" /> has been called
    /// </summary>
    public int LoadCount => _loadedZips.Count;

    /// <summary>
    /// The number of times <see cref="Dispose" /> has been called
    /// </summary>
    public int DisposeCount { get; private set; }

    /// <summary>
    /// Creates a new mock cache that has the given <paramref name="mediaPath" /> and <paramref name="config" />
    /// and no media files
    /// </summary>
    /// <param name="config">The <see cref="IConfig"/> to be returned by <see cref="Import" /></param>
    /// <param name="mediaPath">The value to set <see cref="MediaPath" /> to</param>
    public MockFileHandler(IConfig config, string mediaPath)
    {
        _media = new List<Hotspot.Media>();
        MediaPath = mediaPath;
        Config = config;
    }

    /// <summary>
    /// Creates a new <see cref="MockFileHandler" /> with the given list of media files,
    /// and empty <see cref="IConfig" /> and <see cref="MediaPath" />
    /// </summary>
    /// <param name="files">The list of media files provided to <see cref="CreateContentProvider" /></param>
    public MockFileHandler(List<Hotspot.Media> files)
    {
        _media = files;
        MediaPath = "";
        Config = new Config(Enumerable.Empty<Hotspot>());
    }

    /// <summary>
    /// Creates a new <see cref="MockFileHandler" /> that will search through the given list of media
    /// </summary>
    /// <param name="exception"></param>
    public MockFileHandler(Exception exception) : this(new List<Hotspot.Media>())
    {
        _exception = exception;
    }

    /// <summary>
    /// Adds the <paramref name="zipPath" /> to the list of loaded zips and returns the <see cref="Config" />
    /// </summary>
    /// <param name="zipPath">The theoretical path to the zip file containing media files</param>
    /// <returns><see cref="IConfig" /></returns>
    public IConfig Import(string zipPath)
    {
        _loadedZips.Add(zipPath);
        return Config;
    }

    /// <summary>
    /// Returns the currently stored <see cref="UConfig"/>
    /// </summary>
    /// <returns>Stored <see cref="IConfig"/></returns>
    public IConfig LoadConfig()
    {
        return Config;
    }

    /// <summary>
    /// Returns if <see cref="Config"/> is loaded
    /// </summary>
    /// <returns>Always true as <see cref="IConfig"/> defined in constructor</returns>
    public bool IsConfigImported()
    {
        return true;
    }

    public bool Save(IConfig config)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates a new <see cref="MockContentProvider" />
    /// </summary>
    /// <param name="config">
    /// Theoretical <see cref="IConfig" /> object to use for constructing <see cref="IContentProvider" /> (ignored here)
    /// </param>
    /// <returns>A new <see cref="MockContentProvider" /></returns>
    public IContentProvider CreateContentProvider(IConfig config) =>
        _exception is null ? new MockContentProvider(_media) : new MockContentProvider(_exception);

    /// <summary>
    /// Increases the number of times <see cref="Dispose" /> has been called
    /// </summary>
    public void Dispose()
    {
        DisposeCount++;
    }
}
