using System;
using LibVLCSharp.Shared;
using ReactiveUI;

namespace WallProjections.ViewModels;

public sealed class VideoViewModel : ViewModelBase, IDisposable
{
    private readonly LibVLC _libVlc = new();
    private readonly string _videoPath;
    private bool _isDisposed;
    private MediaPlayer? _mediaPlayer;

    public VideoViewModel(string videoPath)
    {
        _videoPath = videoPath;
        MediaPlayer = new MediaPlayer(_libVlc);
    }

    public MediaPlayer? MediaPlayer
    {
        get => _mediaPlayer;
        private set => this.RaiseAndSetIfChanged(ref _mediaPlayer, value);
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        StopVideo();
        var mediaPlayer = MediaPlayer;
        MediaPlayer = null;
        mediaPlayer?.Dispose();
        _libVlc.Dispose();
    }

    public void PlayVideo()
    {
        MediaPlayer?.Play(new Media(_libVlc, _videoPath));
    }

    private void StopVideo()
    {
        MediaPlayer?.Stop();
        MediaPlayer?.Media?.Dispose();
    }
}
