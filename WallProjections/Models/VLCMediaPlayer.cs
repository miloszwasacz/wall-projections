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
    /// This is a wrapper implementing <see cref="IMediaPlayer" /> around
    /// LibVLCSharp's <see cref="MediaPlayer" /> class
    /// </summary>
    public VLCMediaPlayer(LibVLC libVlc) : base(libVlc)
    {
    }

    public void SetHandle(IPlatformHandle handle)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Hwnd = handle.Handle;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            XWindow = (uint)handle.Handle;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) NsObject = handle.Handle;
    }

    public void DisposeHandle()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Hwnd = IntPtr.Zero;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            XWindow = 0;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) NsObject = IntPtr.Zero;
    }
}
