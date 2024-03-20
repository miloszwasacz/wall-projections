using System;
using System.Runtime.InteropServices;
using System.Threading;
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
    /// A mutex to ensure sequential access to media player's handle
    /// </summary>
    private readonly Mutex _mutex = new();

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
        _mutex.WaitOne();
        if (_isDisposed)
        {
            _mutex.ReleaseMutex();
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Hwnd = handle.Handle;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            NsObject = handle.Handle;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            XWindow = (uint)handle.Handle;

        _mutex.ReleaseMutex();
    }

    /// <inheritdoc />
    public void DisposeHandle()
    {
        _mutex.WaitOne();
        _isDisposed = true;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Hwnd = IntPtr.Zero;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            NsObject = IntPtr.Zero;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            XWindow = 0;

        _mutex.ReleaseMutex();
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        DisposeHandle();
        base.Dispose(disposing);
    }
}
