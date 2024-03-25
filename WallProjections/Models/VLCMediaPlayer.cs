using System;
using System.Runtime.InteropServices;
using Avalonia.Platform;
using LibVLCSharp.Shared;
using WallProjections.Models.Interfaces;

namespace WallProjections.Models;

// ReSharper disable once InconsistentNaming
/// <summary>
/// This is a wrapper implementing <see cref="IMediaPlayer" /> around
/// LibVLCSharp's <see cref="MediaPlayer" /> class
/// </summary>
public sealed class VLCMediaPlayer : MediaPlayer, IMediaPlayer
{
    /// <summary>
    /// Whether the media player has already been disposed
    /// </summary>
    private bool _isDisposed;

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
    protected override void Dispose(bool disposing)
    {
        DisposeHandle();
        base.Dispose(disposing);
    }
}
