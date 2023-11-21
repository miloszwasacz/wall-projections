using System;
using LibVLCSharp.Shared;
using ReactiveUI;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.ViewModels;

public sealed class VideoViewModel : ViewModelBase, IVideoViewModel
{
    private readonly LibVLC _libVlc;
    private bool _isDisposed;
    private bool _isPlaying;
    private IMediaPlayer? _mediaPlayer;

    public VideoViewModel(LibVLC libVlc, IMediaPlayer mediaPlayer)
    {
        _libVlc = libVlc;
        _mediaPlayer = mediaPlayer;
        this.RaisePropertyChanged(nameof(MediaPlayer));
    }

    /// <summary>
    /// The media player used to play the videos
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// If you try to overwrite the MediaPlayer with anything that is not null.
    /// Note that this should only is done once in <see cref="Dispose" /> and <b>shouldn't be done anywhere else</b>!
    /// </exception>
    public IMediaPlayer? MediaPlayer
    {
        get => _mediaPlayer;
        private set
        {
            if (value is not null)
                throw new InvalidOperationException("MediaPlayer cannot be set to a non-null value");
            HasVideos = false;
            this.RaiseAndSetIfChanged(ref _mediaPlayer, null);
        }
    }

    public bool HasVideos
    {
        get => _isPlaying;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isPlaying, value);
            this.RaisePropertyChanged(nameof(Volume));
        }
    }

    public int Volume
    {
        get => HasVideos ? MediaPlayer?.Volume ?? 0 : 0;
        set
        {
            if (MediaPlayer is not null)
                MediaPlayer.Volume = value;
        }
    }

    public bool PlayVideo(string path)
    {
        if (_isDisposed) return false;

        HasVideos = true;
        var media = new Media(_libVlc, path);
        var success = MediaPlayer?.Play(media);
        media.Dispose();
        return success ?? false;
    }

    public void StopVideo()
    {
        if (_isDisposed) return;

        MediaPlayer?.Stop();
        HasVideos = false;
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
    }
}
