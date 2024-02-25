using System;
using Avalonia.Platform;
using WallProjections.Models.Interfaces;

namespace WallProjections.ViewModels.Interfaces.Display;

public interface IVideoViewModel : IDisposable
{
    /// <summary>
    /// The <see cref="IMediaPlayer" /> used to play videos.
    /// </summary>
    public IMediaPlayer? MediaPlayer { get; }

    /// <summary>
    /// Whether or not the viewmodel has a video to display.
    /// </summary>
    public bool HasVideos { get; }

    /// <summary>
    /// The volume of the video.
    /// </summary>
    public int Volume { get; set; }

    /// <summary>
    /// Plays the video at the given path.
    /// </summary>
    /// <param name="path">The path to the video.</param>
    /// <returns>Whether or not the video has successfully started playing.</returns>
    public bool PlayVideo(string path);

    /// <summary>
    /// Stops the currently playing video.
    /// </summary>
    public void StopVideo();
}
