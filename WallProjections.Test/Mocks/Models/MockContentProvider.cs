using WallProjections.Models;
using WallProjections.Models.Interfaces;

namespace WallProjections.Test.Mocks.Models;

public class MockContentProvider : IContentProvider
{
    /// <summary>
    /// The list of media to search through in <see cref="GetMedia" />
    /// </summary>
    private readonly List<Hotspot.Media> _media;

    /// <summary>
    /// The exception to throw when <see cref="GetMedia" /> is called
    /// </summary>
    private readonly Exception? _exception;

    /// <summary>
    /// The last hotspot ID that was requested
    /// </summary>
    /// <remarks><i>null</i> if <see cref="GetMedia" /> has not been called</remarks>
    public int? FileNumber { get; private set; }

    /// <summary>
    /// Creates a new <see cref="MockContentProvider" /> that will search through the given list of media
    /// when <see cref="GetMedia" /> is called
    /// </summary>
    /// <param name="result"></param>
    public MockContentProvider(List<Hotspot.Media> result)
    {
        _media = result;
    }

    /// <summary>
    /// Creates a new <see cref="MockContentProvider" /> that throws an exception when <see cref="GetMedia" /> is called
    /// </summary>
    /// <param name="exception"></param>
    public MockContentProvider(Exception exception) : this(new List<Hotspot.Media>())
    {
        _exception = exception;
    }

    /// <summary>
    /// Looks through the list of media provided in the constructor for the given <paramref name="hotspotId" />
    /// and returns the <see cref="Hotspot.Media" /> object associated with it, or throws an exception
    /// </summary>
    /// <param name="hotspotId">The ID to search for in the media files</param>
    /// <returns>The <see cref="Hotspot.Media" /> object associated with the given <paramref name="hotspotId" /></returns>
    /// <exception cref="Exception">The exception that was passed to the constructor</exception>
    /// <exception cref="IConfig.HotspotNotFoundException">
    /// If the hotspot ID does not exist in the list of media provided in the constructor
    /// </exception>
    /// <inheritdoc />
    public Hotspot.Media GetMedia(int hotspotId)
    {
        if (_exception is not null)
            throw _exception;

        FileNumber = hotspotId;
        return _media.Find(media => media.Id == hotspotId) ??
               throw new IConfig.HotspotNotFoundException(hotspotId);
    }
}
