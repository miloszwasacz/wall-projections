using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    private bool _isDisposed;

    /// <summary>
    /// Mutex for <see cref="HasVideos"/>, <see cref="_hasVideos"/> and <see cref="_isLoaded"/>
    /// </summary>
    private readonly Mutex _stateMutex;

    /// <summary>
    /// Whether or not a videos are queued
    /// </summary>
    private bool _hasVideos;

    /// <summary>
    /// Whether the video player has loaded
    /// </summary>
    private bool _isLoaded;

    private bool _isVisible;

    /// <summary>
    /// All the videos queued to play
    /// </summary>
    private readonly ConcurrentQueue<string> _playQueue;

    /// <summary>
    /// The backing field for <see cref="MediaPlayer" />
    /// </summary>
    private IMediaPlayer? _mediaPlayer;
    
    public VideoViewModel(LibVLC libVlc, IMediaPlayer mediaPlayer)
    {
        _libVlc = libVlc;
        _mediaPlayer = mediaPlayer;
        _stateMutex = new Mutex();
        _playQueue = new ConcurrentQueue<string>();
        _mediaPlayer.EndReached += PlayNextVideoEvent;
        _isVisible = false;

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

    /// <inheritdoc />
    public bool IsVisible
    {
        get => _isVisible;
        set => this.RaiseAndSetIfChanged(ref _isVisible, value);
    }

    /// <inheritdoc />
    public bool HasVideos
    {
        get
        {
            _stateMutex.WaitOne();
            var temp = _hasVideos;
            _stateMutex.ReleaseMutex();
            return temp;
        }
        private set
        {
            _stateMutex.WaitOne();
            this.RaiseAndSetIfChanged(ref _hasVideos, value);
            _stateMutex.ReleaseMutex();
            this.RaisePropertyChanged(nameof(Volume));
        }
    }

    /// <inheritdoc />
    public int Volume
    {
        get => HasVideos ? MediaPlayer?.Volume ?? 0 : 0;
        set
        {
            if (MediaPlayer is not null)
                MediaPlayer.Volume = value;
        }
    }

    public void Loaded()
    {
        _stateMutex.WaitOne();
        if (HasVideos)
        {
            PlayNextVideo();
        }
        _isLoaded = true;
        IsVisible = true;
        _stateMutex.ReleaseMutex();
    }

    /// <inheritdoc />
    public bool PlayVideo(string path)
    {
        return PlayVideos(new List<string>{ path });
    }

    public bool PlayVideos(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            _playQueue.Enqueue(path);
        }

        _stateMutex.WaitOne();
        if (_isLoaded && !HasVideos)
        {
            PlayNextVideo();
        }
        
        HasVideos = true;
        _stateMutex.ReleaseMutex();

        return true;
    }

    /// <inheritdoc />
    public void StopVideo()
    {
        if (_isDisposed) return;

        MediaPlayer?.Stop();
        HasVideos = false;
    }
    
    /// <inheritdoc />
    public bool PlayNextVideo()
    {

        // End of queue reached
        if (_isDisposed || MediaPlayer is null || !_playQueue.TryDequeue(out var nextVideo))
        {
            HasVideos = false;
            return false;
        }

        var media = new Media(_libVlc, nextVideo);
        var success = MediaPlayer.Play(media);
        _hasVideos = true;
        media.Dispose();
        return success;
    }

    /// <summary>
    /// Event handler wrapper for playing the next video in the sequence when one ends.
    /// </summary>
    /// <param name="sender">Object who sent request</param>
    /// <param name="e">Arguments for the event</param>
    private void PlayNextVideoEvent(object? sender, EventArgs e)
    {
        var success = PlayNextVideo();

        if (!success) Console.WriteLine("Could not play next video");
    }

    public void Dispose()
    {
        Console.WriteLine("Disposing VideoViewModel");
        if (_isDisposed)
            return;

        IsVisible = false;

        StopVideo();
        MediaPlayer.DisposeHandle();
        MediaPlayer.EndReached -= PlayNextVideoEvent;

        _isDisposed = true;
        var mediaPlayer = MediaPlayer;
        MediaPlayer = null;
        mediaPlayer?.Dispose();
    }
}
