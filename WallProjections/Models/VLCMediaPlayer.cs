using LibVLCSharp.Shared;

namespace WallProjections.Models;

// ReSharper disable once InconsistentNaming
/// <inheritdoc cref="LibVLCSharp.Shared.MediaPlayer"/>
/// <remarks>
/// This is a wrapper around LibVLCSharp's <see cref="LibVLCSharp.Shared.MediaPlayer" /> class
/// </remarks>
public sealed class VLCMediaPlayer : MediaPlayer
{
    /// <summary>
    /// The underlying <see cref="LibVLCSharp.Shared.MediaPlayer" /> instance
    /// </summary>
    public override LibVLCSharp.Shared.MediaPlayer Player { get; }

    /// <inheritdoc cref="LibVLCSharp.Shared.MediaPlayer(LibVLCSharp.Shared.LibVLC)"/>
    /// <remarks>
    /// This is a wrapper around LibVLCSharp's <see cref="LibVLCSharp.Shared.MediaPlayer" /> class
    /// </remarks>
    public VLCMediaPlayer(LibVLC libVlc)
    {
        Player = new LibVLCSharp.Shared.MediaPlayer(libVlc);
    }

    /// <inheritdoc cref="LibVLCSharp.Shared.MediaPlayer.Play(LibVLCSharp.Shared.Media)" />
    public override void Play(Media media)
    {
        Player.Play(media);
    }

    /// <inheritdoc cref="LibVLCSharp.Shared.MediaPlayer.Stop" />
    public override void Stop()
    {
        Player.Stop();
    }

    /// <inheritdoc cref="LibVLCSharp.Shared.MediaPlayer.Dispose" />
    public override void Dispose()
    {
        Player.Dispose();
    }
}
