using LibVLCSharp.Shared;
using WallProjections.Models.Interfaces;

namespace WallProjections.Models;

// ReSharper disable once InconsistentNaming
/// <summary>
/// This is a wrapper implementing <see cref="IMediaPlayer"/> around
/// LibVLCSharp's <see cref="MediaPlayer" /> class
/// </summary>
public sealed class VLCMediaPlayer : MediaPlayer, IMediaPlayer
{
    /// <summary>
    /// This is a wrapper implementing <see cref="IMediaPlayer"/> around
    /// LibVLCSharp's <see cref="MediaPlayer" /> class
    /// </summary>
    public VLCMediaPlayer(LibVLC libVlc) : base(libVlc)
    {
    }

    // /// <inheritdoc cref="LibVLCSharp.Shared.MediaPlayer.Play(LibVLCSharp.Shared.Media)" />
    // public new bool Play(Media media) => base.Play(media) && IsPlaying;
    //
    // /// <inheritdoc cref="LibVLCSharp.Shared.MediaPlayer.Stop" />
    // public new void Stop() => base.Stop();
}
