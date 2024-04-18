using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks.Models;
using WallProjections.ViewModels.Display;
using WallProjections.ViewModels.Interfaces.Display;

namespace WallProjections.Test.Mocks.ViewModels.Display;

/// <summary>
/// A mock of <see cref="VideoViewModel" /> for injecting into <see cref="DisplayViewModel" />
/// </summary>
public sealed class MockVideoViewModel : IVideoViewModel
{
    /// <summary>
    /// This event can be raised using <see cref="InvokeAllVideosFinished" />
    /// </summary>
    public event EventHandler? AllVideosFinished;

    private readonly List<(string path, bool hasPlayed)> _videoPaths = new();

    /// <summary>
    /// The backing field for <see cref="MediaPlayer" />
    /// </summary>
    private readonly IMediaPlayer? _mediaPlayer;

    /// <summary>
    /// The backing field for <see cref="IsVisible" />
    /// </summary>
    private bool _isVisible;

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
    /// A list of paths to the videos the viewmodel has played or will play
    /// </summary>
    public IReadOnlyList<string> VideoPaths => _videoPaths.Select(v => v.path).ToList();

    /// <summary>
    /// Determines if <see cref="MediaPlayer" /> is null or not (<i>true</i> means not <i>null</i>)
    /// </summary>
    public bool CanPlay { get; set; }

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
    /// Whether <see cref="MarkLoaded" /> has been called
    /// </summary>
    public bool IsLoaded { get; private set; }

    /// <summary>
    /// A mock of <see cref="WallProjections.Models.VLCMediaPlayer" /> if <see cref="CanPlay" /> is <i>true</i>,
    /// <i>null</i> otherwise
    /// </summary>
    public IMediaPlayer? MediaPlayer => CanPlay ? _mediaPlayer : null;

    /// <summary>
    /// Returns <see cref="CanPlay" />
    /// </summary>
    public bool HasVideos => CanPlay;

    public bool IsVisible => _isVisible && HasVideos;

    public int Volume { get; set; }

    public void MarkLoaded()
    {
        IsLoaded = true;
    }

    /// <summary>
    /// Increases the number of times <see cref="PlayVideo" /> has been called
    /// and adds <paramref name="path" /> to <see cref="VideoPaths" />
    /// </summary>
    /// <returns><i>True</i> if <see cref="MediaPlayer" /> is not <i>null</i></returns>
    public bool PlayVideo(string path) => PlayVideos(new[] { path });

    public bool PlayVideos(IEnumerable<string> paths)
    {
        _videoPaths.AddRange(paths.Select(p => (p, false)));
        _isVisible = true;
        return true;
    }

    /// <summary>
    /// Sets the first unplayed video in <see cref="VideoPaths" /> to played
    /// </summary>
    /// <returns>Whether a video was played</returns>
    public bool PlayNextVideo()
    {
        for (var i = 0; i < _videoPaths.Count; i++)
        {
            if (_videoPaths[i].hasPlayed) continue;

            _videoPaths[i] = (_videoPaths[i].path, true);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Increases the number of times <see cref="StopVideo" /> has been called
    /// </summary>
    public void StopVideo()
    {
        StopCount++;
        _isVisible = false;
        CanPlay = false;
    }

    /// <summary>
    /// Invokes <see cref="AllVideosFinished" />
    /// </summary>
    public void InvokeAllVideosFinished() => AllVideosFinished?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Calls <see cref="StopVideo" /> and increases the number of times <see cref="Dispose" /> has been called
    /// </summary>
    public void Dispose()
    {
        StopVideo();
        DisposeCount++;
    }
}
