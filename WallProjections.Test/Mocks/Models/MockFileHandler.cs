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
    /// The backing field for <see cref="ExportedZips"/>
    /// </summary>
    private readonly List<string> _exportedZips = new();

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
    /// The result of all "modification" methods <i>(see <see cref="MockFileHandler(bool)" />)</i>.
    /// </summary>
    private readonly bool _modificationOutputs = true;

    /// <summary>
    /// The <see cref="IConfig" /> to be returned by <see cref="ImportConfig" />
    /// </summary>
    public IConfig Config { get; set; }

    /// <summary>
    /// A list of all the zip files that have been loaded using <see cref="ImportConfig" />
    /// </summary>
    public IReadOnlyList<string> LoadedZips => _loadedZips;

    /// <summary>
    /// A list of all the zip files that have been loaded using <see cref="ExportConfig" />
    /// </summary>
    public IReadOnlyList<string> ExportedZips => _exportedZips;

    /// <summary>
    /// The number of times <see cref="ImportConfig" /> has been called
    /// </summary>
    public int LoadCount => _loadedZips.Count;

    /// <summary>
    /// Creates a new mock cache that uses the given <paramref name="config" /> and no media files
    /// </summary>
    /// <param name="config">The <see cref="IConfig"/> to be returned by <see cref="ImportConfig" /></param>
    public MockFileHandler(IConfig config)
    {
        _media = new List<Hotspot.Media>();
        Config = config;
    }

    /// <summary>
    /// Creates a new <see cref="MockFileHandler" /> with the given list of media files,
    /// and empty <see cref="IConfig" />
    /// </summary>
    /// <param name="files">The list of media files provided to <see cref="CreateContentProvider" /></param>
    public MockFileHandler(List<Hotspot.Media> files)
    {
        _media = files;
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
    /// Creates a new <see cref="MockFileHandler" /> that will always return the given <paramref name="modificationOutputs" />
    /// when <see cref="ImportConfig" />, <see cref="ExportConfig" />, and <see cref="LoadConfig" /> are called.
    /// </summary>
    /// <param name="modificationOutputs">The result of all "modification" methods</param>
    public MockFileHandler(bool modificationOutputs) : this(new List<Hotspot.Media>())
    {
        _modificationOutputs = modificationOutputs;
    }

    /// <summary>
    /// Sets the <see cref="Config" /> to the given <paramref name="config" />
    /// and returns <i>true</i>, or a custom result <i>(see <see cref="MockFileHandler(bool)" />)</i>.
    /// </summary>
    /// <returns>
    /// <i>true</i>, or the custom result if the mock was created with <i>false</i>
    /// for <see cref="MockFileHandler(bool)" />
    /// </returns>
    /// <exception cref="Exception">
    /// If the mock was created with an <see cref="Exception" /> for <see cref="MockFileHandler(Exception)" />
    /// </exception>
    public bool SaveConfig(IConfig config)
    {
        if (_exception is not null)
            throw _exception;

        Config = config;
        return _modificationOutputs;
    }

    /// <summary>
    /// Adds the <paramref name="filePath" /> to the list of loaded zips and returns the <see cref="Config" />
    /// </summary>
    /// <param name="filePath">The theoretical path to the zip file containing media files</param>
    /// <returns>
    /// <see cref="IConfig" />, or <i>null</i> if the mock was created with <i>false</i>
    /// for <see cref="MockFileHandler(bool)" />
    /// </returns>
    /// <exception cref="Exception">
    /// If the mock was created with an <see cref="Exception" /> for <see cref="MockFileHandler(Exception)" />
    /// </exception>
    public IConfig? ImportConfig(string filePath)
    {
        if (_exception is not null)
            throw _exception;

        _loadedZips.Add(filePath);
        return _modificationOutputs ? Config : null;
    }

    /// <summary>
    /// Adds the <paramref name="exportPath"/> to the list of loaded zips and returns true"/>
    /// </summary>
    /// <param name="exportPath">The theoretical path to save the config and media.</param>
    /// <returns>
    /// <i>true</i>, or the custom result if the mock was created with <i>false</i>
    /// for <see cref="MockFileHandler(bool)" />
    /// </returns>
    /// <exception cref="Exception">
    /// If the mock was created with an <see cref="Exception" /> for <see cref="MockFileHandler(Exception)" />
    /// </exception>
    public bool ExportConfig(string exportPath)
    {
        if (_exception is not null)
            throw _exception;

        _exportedZips.Add(exportPath);
        return _modificationOutputs;
    }

    /// <summary>
    /// Returns the currently stored <see cref="IConfig"/>
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

    /// <summary>
    /// Creates a new <see cref="MockContentProvider" />
    /// </summary>
    /// <param name="config">
    /// Theoretical <see cref="IConfig" /> object to use for constructing <see cref="IContentProvider" /> (ignored here)
    /// </param>
    /// <returns>A new <see cref="MockContentProvider" /></returns>
    public IContentProvider CreateContentProvider(IConfig config) =>
        _exception is null ? new MockContentProvider(_media) : new MockContentProvider(_exception);
}
