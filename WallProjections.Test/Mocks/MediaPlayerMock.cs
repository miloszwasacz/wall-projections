using System.Reflection;
using LibVLCSharp.Shared;
using WallProjections.ViewModels;
using MediaPlayer = WallProjections.Models.MediaPlayer;

namespace WallProjections.Test.Mocks;

/// <summary>
/// A mock of <see cref="MediaPlayer"/> for injecting into <see cref="WallProjections.ViewModels.VideoViewModel"/>
/// </summary>
public sealed class MediaPlayerMock : MediaPlayer
{
    /// <summary>
    /// A stub of <see cref="LibVLCSharp.Shared.MediaPlayer"/> which should not be used
    /// </summary>
    public override LibVLCSharp.Shared.MediaPlayer Player => null!;

    private readonly List<string> _mrlList = new();
    private int _stoppedCount;
    private int _disposedCount;


    /// <summary>
    /// Adds the MRL of the given media to the list of played media
    /// </summary>
    /// <param name="media">Media whose MRL will be added to the list of played media</param>
    /// <returns>Always returns true</returns>
    public override bool Play(Media media)
    {
        _mrlList.Add(media.Mrl);
        media.Dispose();
        return true;
    }

    /// <summary>
    /// Increases the number of times the media player has been stopped
    /// </summary>
    public override void Stop()
    {
        _stoppedCount++;
    }

    /// <summary>
    /// Increases the number of times the media player has been disposed
    /// </summary>
    public override void Dispose()
    {
        _disposedCount++;
    }

    /// <summary>
    /// Injects a <see cref="MediaPlayerMock"/> into the provided <see cref="VideoViewModel"/>
    /// by replacing the <see cref="MediaPlayer"/>
    /// </summary>
    /// <param name="vm">The target of injection</param>
    /// <returns>The mock media player</returns>
    /// <exception cref="NullReferenceException">Could not inject the mock because the required field was missing</exception>
    public static MediaPlayerMock InjectInto(VideoViewModel vm)
    {
        var mediaPlayerMock = new MediaPlayerMock();
        var mp = vm.MediaPlayer;
        var props = vm.GetType().GetField("_mediaPlayer", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?? throw new NullReferenceException("Could not find _mediaPlayer field");
        props.SetValue(vm, mediaPlayerMock);
        mp?.Dispose();
        return mediaPlayerMock;
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
