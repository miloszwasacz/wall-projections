using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks.Models;
using WallProjections.ViewModels.Display;
using WallProjections.ViewModels.Interfaces.Display;

namespace WallProjections.Test.Mocks.ViewModels;

/// <summary>
/// A mock of <see cref="VideoViewModel" /> for injecting into <see cref="DisplayViewModel" />
/// </summary>
public sealed class MockVideoViewModel : IVideoViewModel
{
    private readonly List<string> _videoPaths = new();

    /// <summary>
    /// The backing field for <see cref="MediaPlayer" />
    /// </summary>
    private IMediaPlayer? _mediaPlayer;

    /// <summary>
    /// A mock of <see cref="VideoViewModel" /> for injecting into <see cref="DisplayViewModel" />.
    /// Sets <see cref="MediaPlayer" /> to <paramref name="mediaPlayer" /> if not <i>null</i>,
    /// or a new <see cref="MockMediaPlayer" /> otherwise
    /// </summary>
    public MockVideoViewModel(IMediaPlayer? mediaPlayer = null)
    {
        _mediaPlayer = mediaPlayer ?? new MockMediaPlayer();
    }

    /// <summary>
    /// A list of paths to the videos the viewmodel has played
    /// </summary>
    public IReadOnlyList<string> VideoPaths => _videoPaths;

    /// <summary>
    /// Determines if <see cref="MediaPlayer" /> is null or not (<i>true</i> means not <i>null</i>)
    /// </summary>
    public bool CanPlay { get; set; } = false;

    /// <summary>
    /// The number of times <see cref="PlayVideo" /> has been called
    /// </summary>
    public int PlayCount => _videoPaths.Count;

    /// <summary>
    /// The number of times <see cref="StopVideo" /> has been called
    /// </summary>
    public int StopCount { get; private set; }

    /// <summary>
    /// The number of times <see cref="Dispose" /> has been called
    /// </summary>
    public int DisposeCount { get; private set; }

    /// <summary>
    /// A mock of <see cref="WallProjections.Models.VLCMediaPlayer" /> if <see cref="CanPlay" /> is <i>true</i>,
    /// <i>null</i> otherwise
    /// </summary>
    public IMediaPlayer? MediaPlayer => CanPlay ? _mediaPlayer : null;

    /// <summary>
    /// Returns <see cref="CanPlay" />
    /// </summary>
    public bool HasVideos => CanPlay;

    public int Volume { get; set; }

    /// <summary>
    /// Increases the number of times <see cref="PlayVideo" /> has been called
    /// and adds <paramref name="path" /> to <see cref="VideoPaths" />
    /// </summary>
    /// <returns><i>True</i> if <see cref="MediaPlayer" /> is not <i>null</i></returns>
    public bool PlayVideo(string path)
    {
        _videoPaths.Add(path);
        return MediaPlayer is not null;
    }

    /// <summary>
    /// Increases the number of times <see cref="StopVideo" /> has been called
    /// </summary>
    public void StopVideo()
    {
        StopCount++;
    }

    /// <summary>
    /// Calls <see cref="StopVideo" /> and increases the number of times <see cref="Dispose" /> has been called
    /// </summary>
    public void Dispose()
    {
        StopVideo();
        DisposeCount++;
    }
}
