using System;
using LibVLCSharp.Shared;
using ReactiveUI;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.ViewModels;

public sealed class VideoViewModel : ViewModelBase, IVideoViewModel
{
    private readonly LibVLC _libVlc;
    private readonly string _videoPath;
    private IMediaPlayer? _mediaPlayer;
    private bool _isDisposed;

    public VideoViewModel(string videoPath, LibVLC libVlc, IMediaPlayer mediaPlayer)
    {
        _videoPath = videoPath;
        _libVlc = libVlc;
        _mediaPlayer = mediaPlayer;
    }

    /// <summary>
    /// The media player used to play the videos
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// If you try to overwrite the MediaPlayer with anything that is not null
    /// </exception>
    public IMediaPlayer? MediaPlayer
    {
        get => _mediaPlayer;
        private set
        {
            if (value is not null)
                throw new InvalidOperationException("MediaPlayer cannot be set to a non-null value");
            this.RaiseAndSetIfChanged(ref _mediaPlayer, null);
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
    }

    public bool PlayVideo()
    {
        var media = new Media(_libVlc, _videoPath);
        var success = _mediaPlayer?.Play(media);
        media.Dispose();
        return success ?? false;
    }

    public void StopVideo()
    {
        _mediaPlayer?.Stop();
    }
}
