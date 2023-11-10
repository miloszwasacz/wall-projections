using System;
using LibVLCSharp.Shared;
using ReactiveUI;
using WallProjections.Models;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;
using MediaPlayerWrapper = WallProjections.Models.MediaPlayer;

namespace WallProjections.ViewModels;

public sealed class VideoViewModel : ViewModelBase, IDisposable
{
    private readonly LibVLC _libVlc = new();
    private readonly string _videoPath;
    private bool _isDisposed;
    private MediaPlayerWrapper? _mediaPlayer;

    public VideoViewModel(string videoPath)
    {
        _videoPath = videoPath;
        _mediaPlayer = new VLCMediaPlayer(_libVlc);
    }

    public MediaPlayer? MediaPlayer
    {
        get => _mediaPlayer?.Player;
        private set
        {
            if (value is not null)
                throw new InvalidOperationException("MediaPlayer cannot be set to a non-null value");
            _mediaPlayer = null;
            this.RaisePropertyChanged();
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        StopVideo();
        var mediaPlayer = _mediaPlayer;
        MediaPlayer = null;
        mediaPlayer?.Dispose();
        _libVlc.Dispose();
    }

    public void PlayVideo()
    {
        var media = new Media(_libVlc, _videoPath);
        _mediaPlayer?.Play(media);
        media.Dispose();
    }

    public void StopVideo()
    {
        _mediaPlayer?.Stop();
    }
}
