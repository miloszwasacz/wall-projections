using LibVLCSharp.Shared;
using WallProjections.Models;
using WallProjections.Models.Interfaces;

namespace WallProjections.Test.Mocks;

/// <summary>
/// A mock of <see cref="VLCMediaPlayer"/> for injecting into <see cref="WallProjections.ViewModels.VideoViewModel"/>
/// </summary>
public sealed class MockMediaPlayer : IMediaPlayer
{
    private readonly bool _fileExists;
    private readonly List<string> _mrlList = new();
    private int _stoppedCount;
    private int _disposedCount;

    /// <summary>
    /// A mock of <see cref="VLCMediaPlayer"/> for injecting into <see cref="WallProjections.ViewModels.VideoViewModel"/>
    /// </summary>
    /// <param name="fileExists">Determines the return value of <see cref="MockMediaPlayer.Play"/></param>
    public MockMediaPlayer(bool fileExists = true)
    {
        _fileExists = fileExists;
    }

    /// <summary>
    /// Adds the MRL of the given media to the list of played media
    /// </summary>
    /// <param name="media">Media whose MRL will be added to the list of played media</param>
    /// <returns>True if the <see cref="MockMediaPlayer(bool)"/> was constructed with <code>true</code></returns>
    public bool Play(Media media)
    {
        if (_fileExists)
            _mrlList.Add(media.Mrl);
        media.Dispose();
        return _fileExists;
    }

    /// <summary>
    /// Increases the number of times the media player has been stopped
    /// </summary>
    public void Stop()
    {
        _stoppedCount++;
    }

    /// <summary>
    /// Increases the number of times the media player has been disposed
    /// </summary>
    public void Dispose()
    {
        _disposedCount++;
    }

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
