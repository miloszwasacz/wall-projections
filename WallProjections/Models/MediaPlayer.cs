using System;
using LibVLCSharp.Shared;

namespace WallProjections.Models;

/// <inheritdoc cref="LibVLCSharp.Shared.MediaPlayer"/>
/// <remarks>
/// This is a wrapper around LibVLCSharp's <see cref="LibVLCSharp.Shared.MediaPlayer" /> class to allow for mocking.
/// </remarks>
public abstract class MediaPlayer : IDisposable
{
    /// <summary>
    /// The underlying <see cref="LibVLCSharp.Shared.MediaPlayer" /> instance
    /// </summary>
    public abstract LibVLCSharp.Shared.MediaPlayer Player { get; }

    /// <inheritdoc cref="LibVLCSharp.Shared.MediaPlayer.Play(LibVLCSharp.Shared.Media)" />
    public abstract void Play(Media media);

    /// <inheritdoc cref="LibVLCSharp.Shared.MediaPlayer.Stop" />
    public abstract void Stop();

    /// <inheritdoc cref="LibVLCSharp.Shared.MediaPlayer.Dispose" />
    public abstract void Dispose();
}
