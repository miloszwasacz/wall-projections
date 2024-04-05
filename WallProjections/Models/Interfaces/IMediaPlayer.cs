using System;
using Avalonia.Platform;
using LibVLCSharp.Shared;

namespace WallProjections.Models.Interfaces;

/// <inheritdoc cref="MediaPlayer" />
/// <remarks>
/// This is a wrapper around LibVLCSharp's <see cref="MediaPlayer" /> class to allow for mocking.
/// </remarks>
public interface IMediaPlayer : IDisposable
{
    /// <inheritdoc cref="MediaPlayer.EndReached"/>
    public event EventHandler<EventArgs> EndReached;

    /// <summary>
    /// The Width and Height of the video currently being played.
    /// </summary>
    public (uint Width, uint Height)? VideoSize { get; }

    /// <inheritdoc cref="MediaPlayer.Volume" />
    public int Volume { get; set; }

    /// <inheritdoc cref="MediaPlayer.IsPlaying" />
    public bool IsPlaying { get; }

    /// <inheritdoc cref="MediaPlayer.Play(Media)" />
    public bool Play(Media media);

    /// <inheritdoc cref="MediaPlayer.Stop" />
    public void Stop();

    /// <summary>
    /// Sets the platform-specific handle for the media player.
    /// </summary>
    /// <param name="handle">The platform-specific video handle.</param>
    public void SetHandle(IPlatformHandle handle);

    /// <summary>
    /// Disposes of the handle set by <see cref="SetHandle"/>.
    /// </summary>
    public void DisposeHandle();
}
