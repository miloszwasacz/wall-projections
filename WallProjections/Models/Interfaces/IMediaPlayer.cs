using System;
using Avalonia.Platform;
using LibVLCSharp.Shared;

namespace WallProjections.Models.Interfaces;

/// <inheritdoc cref="MediaPlayer" />
/// <remarks>
/// This is a wrapper around LibVLCSharp's <see cref="MediaPlayer" /> class to allow for mocking
/// </remarks>
public interface IMediaPlayer : IDisposable
{
    /// <inheritdoc cref="MediaPlayer.Volume" />
    public int Volume { get; set; }

    /// <inheritdoc cref="MediaPlayer.Play(Media)" />
    public bool Play(Media media);

    /// <inheritdoc cref="MediaPlayer.Stop" />
    public void Stop();

    public void SetHandle(IPlatformHandle handle);

    public void DisposeHandle();
}
