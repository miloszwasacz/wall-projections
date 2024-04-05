using Avalonia.Platform;
using LibVLCSharp.Shared;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Display;

namespace WallProjections.Test.Mocks.Models;

/// <summary>
/// A mock of <see cref="VLCMediaPlayer"/> for injecting into <see cref="VideoViewModel"/>
/// </summary>
public sealed class MockMediaPlayer : IMediaPlayer
{
    private readonly bool _fileExists;
    private readonly List<string> _mrlList = new();
    private int _stoppedCount;
    private int _disposedCount;
    private IPlatformHandle? _handle;

    /// <summary>
    /// A mock of <see cref="VLCMediaPlayer"/> for injecting into <see cref="VideoViewModel"/>
    /// </summary>
    /// <param name="fileExists">Determines the return value of <see cref="MockMediaPlayer.Play"/></param>
    public MockMediaPlayer(bool fileExists = true)
    {
        _fileExists = fileExists;
    }

    public event EventHandler<EventArgs>? EndReached;

    /// <inheritdoc />
    /// <remarks>Has to be set manually</remarks>
    public (uint Width, uint Height)? VideoSize { get; set; }

    public int Volume { get; set; }

    public bool IsPlaying { get; private set; }

    /// <summary>
    /// The path to the last <see cref="Play">played</see> video
    /// </summary>
    public string? LastPlayedVideo => _mrlList.LastOrDefault();

    /// <summary>
    /// Adds the MRL of the given media to the list of played media
    /// and invokes <see cref="EndReached"/> after 100 ms (asynchronously)
    /// </summary>
    /// <param name="media">Media whose MRL will be added to the list of played media</param>
    /// <returns>True if the <see cref="MockMediaPlayer(bool)"/> was constructed with <i>true</i></returns>
    public bool Play(Media media)
    {
        if (_fileExists)
            _mrlList.Add(media.Mrl);
        media.Dispose();
        IsPlaying = true;
        return _fileExists;
    }

    /// <summary>
    /// Increases the number of times the media player has been stopped
    /// </summary>
    public void Stop()
    {
        IsPlaying = false;
        _stoppedCount++;
    }

    public void SetHandle(IPlatformHandle handle)
    {
        _handle = handle;
    }

    public void DisposeHandle()
    {
        _handle = null;
        IsPlaying = false;
    }

    /// <summary>
    /// Increases the number of times the media player has been disposed
    /// </summary>
    public void Dispose()
    {
        DisposeHandle();
        _disposedCount++;
    }

    /// <summary>
    /// Invokes <see cref="EndReached"/>
    /// </summary>
    public void MarkVideoAsEnded() => EndReached?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Whether <see cref="SetHandle" /> has been called and <see cref="DisposeHandle" /> has not been called
    /// </summary>
    public bool HasHandle() => _handle != null;

    /// <summary>
    /// Checks if the given MRL is the only one that has been played
    /// </summary>
    /// <param name="mrl">The MRL to check</param>
    public bool HasPlayedOnly(string mrl)
    {
        var fullMrl = new Media(new LibVLC(), mrl).Mrl;
        var result = _mrlList.Count == 1 && _mrlList.Contains(fullMrl);
        _mrlList.Clear();
        return result;
    }

    /// <summary>
    /// Checks if the media player has been stopped the given number of times
    /// </summary>
    /// <param name="times"></param>
    public bool HasStopped(int times = 1)
    {
        var result = _stoppedCount == times;
        _stoppedCount = 0;
        return result;
    }

    /// <summary>
    /// Checks if the media player has not been disposed
    /// </summary>
    public bool HasNotBeenDisposed() => _disposedCount == 0;

    /// <summary>
    /// Checks if the media player has been disposed exactly once
    /// </summary>
    public bool HasBeenDisposedOnce() => _disposedCount == 1;
}
