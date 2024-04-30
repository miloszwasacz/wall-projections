using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Platform;
using LibVLCSharp.Shared;
using WallProjections.Models.Interfaces;

namespace WallProjections.Models;

// ReSharper disable once InconsistentNaming
/// <summary>
/// This is a wrapper implementing <see cref="IMediaPlayer" /> around
/// LibVLCSharp's <see cref="MediaPlayer" /> class
/// </summary>
public sealed class VLCMediaPlayer : MediaPlayer, IMediaPlayer, INotifyPropertyChanged
{
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Whether the media player has already been disposed
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// The backing field for <see cref="VideoSize" />
    /// </summary>
    private (uint, uint)? _videoSize;

    /// <inheritdoc />
    public (uint, uint)? VideoSize
    {
        get => _videoSize;
        private set
        {
            _videoSize = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// This is a wrapper implementing <see cref="IMediaPlayer" /> around
    /// LibVLCSharp's <see cref="MediaPlayer" /> class
    /// </summary>
    public VLCMediaPlayer(LibVLC libVlc) : base(libVlc)
    {
    }

    /// <inheritdoc />
    public void SetHandle(IPlatformHandle handle)
    {
        lock (this)
        {
            if (_isDisposed)
                return;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Hwnd = handle.Handle;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                NsObject = handle.Handle;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                XWindow = (uint)handle.Handle;
        }
    }

    /// <inheritdoc />
    public void DisposeHandle()
    {
        lock (this)
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Hwnd = IntPtr.Zero;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                NsObject = IntPtr.Zero;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                XWindow = 0;
        }
    }

    /// <inheritdoc />
    public new bool Play(Media media)
    {
        var result = base.Play(media);

        // Pause for a short time to allow the size to be calculated
        Pause();

        // Wait for the video to load
        Task.Delay(50).Wait();

        // Get the video size
        uint width = 0, height = 0;
        if (Size(0, ref width, ref height))
            VideoSize = (width, height);
        else
            VideoSize = null;

        // Resume playback
        return result && base.Play();
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        DisposeHandle();
        base.Dispose(disposing);
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged" /> event
    /// </summary>
    /// <param name="propertyName">The name of the property that changed</param>
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
