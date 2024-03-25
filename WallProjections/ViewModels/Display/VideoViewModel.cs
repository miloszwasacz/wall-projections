using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using LibVLCSharp.Shared;
using ReactiveUI;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;

namespace WallProjections.ViewModels.Display;

/// <inheritdoc cref="IVideoViewModel" />
public sealed class VideoViewModel : ViewModelBase, IVideoViewModel
{
    /// <summary>
    /// The <see cref="LibVLC" /> object used to play the videos
    /// </summary>
    private readonly LibVLC _libVlc;

    /// <summary>
    /// Whether or not this <see cref="VideoViewModel" /> has been disposed
    /// </summary>
    /// <remarks>
    /// Remember to use <i>lock (this)</i> when accessing this field
    /// </remarks>
    private bool _isDisposed;

    /// <summary>
    /// Whether the video player has loaded
    /// </summary>
    /// <remarks>
    /// Remember to use <i>lock (this)</i> when accessing this field
    /// </remarks>
    private bool _isLoaded;

    /// <summary>
    /// The backing field for <see cref="IsVisible" />
    /// </summary>
    private bool _isVisible;

    /// <summary>
    /// All the videos queued to play
    /// </summary>
    private readonly ConcurrentQueue<string> _playQueue;

    /// <summary>
    /// The backing field for <see cref="MediaPlayer" />
    /// </summary>
    private IMediaPlayer? _mediaPlayer;

    /// <summary>
    /// Creates a new <see cref="VideoViewModel" /> with the given <see cref="IMediaPlayer" />
    /// </summary>
    /// <param name="libVlc"></param>
    /// <param name="mediaPlayer"></param>
    public VideoViewModel(LibVLC libVlc, IMediaPlayer mediaPlayer)
    {
        _libVlc = libVlc;
        _mediaPlayer = mediaPlayer;
        _playQueue = new ConcurrentQueue<string>();
        _mediaPlayer.EndReached += PlayNextVideoEvent;

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

            _playQueue.Clear();
            IsVisible = false;
            this.RaiseAndSetIfChanged(ref _mediaPlayer, null);
        }
    }

    /// <inheritdoc />
    public bool IsVisible
    {
        get => _isVisible;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isVisible, value);
            this.RaisePropertyChanged(nameof(Volume));
        }
    }

    /// <inheritdoc />
    public bool HasVideos => !_playQueue.IsEmpty;

    /// <inheritdoc />
    public int Volume
    {
        get => IsVisible ? MediaPlayer?.Volume ?? 0 : 0;
        set
        {
            if (MediaPlayer is not null)
                MediaPlayer.Volume = value;
        }
    }

    /// <inheritdoc />
    public void MarkLoaded()
    {
        lock (this)
        {
            if (HasVideos)
                PlayNextVideo();

            _isLoaded = true;
        }
    }

    /// <inheritdoc />
    public bool PlayVideos(IEnumerable<string> paths)
    {
        foreach (var path in paths)
            _playQueue.Enqueue(path);

        lock (this)
        {
            var success = false;
            if (_isLoaded && HasVideos)
                success = PlayNextVideo();

            IsVisible = success;
            return success;
        }
    }

    /// <inheritdoc />
    public void StopVideo()
    {
        lock (this)
        {
            if (_isDisposed)
                return;

            ForceStopVideo();
        }
    }

    /// <summary>
    /// <inheritdoc cref="StopVideo" />(but without if the viewmodel has been disposed).
    /// <b>SHOULD ONLY BE USED IN <see cref="StopVideo" /> and <see cref="Dispose" />!</b>
    /// </summary>
    private void ForceStopVideo()
    {
        IsVisible = false;
        MediaPlayer?.Stop();
    }

    /// <inheritdoc />
    public bool PlayNextVideo()
    {
        lock (this)
        {
            if (_isDisposed || MediaPlayer is null || !_playQueue.TryDequeue(out var nextVideo))
            {
                // End of queue reached
                IsVisible = false;
                return false;
            }

            var media = new Media(_libVlc, nextVideo);
            var success = MediaPlayer.Play(media);
            IsVisible = success;
            media.Dispose();
            return success;
        }
    }

    /// <summary>
    /// Event handler wrapper for playing the next video in the sequence when one ends.
    /// </summary>
    /// <param name="sender">Object who sent request</param>
    /// <param name="e">Arguments for the event</param>
    private void PlayNextVideoEvent(object? sender, EventArgs e)
    {
        var success = PlayNextVideo();

        //TODO Log to file
        if (!success)
            Console.WriteLine("Could not play next video");
    }

    public void Dispose()
    {
        lock (this)
        {
            if (_isDisposed)
                return;

            ForceStopVideo();
            _isDisposed = true;
        }

        var player = MediaPlayer;
        MediaPlayer = null;
        if (player is null) return;
        player.EndReached -= PlayNextVideoEvent;
        player.Dispose();
    }
}
