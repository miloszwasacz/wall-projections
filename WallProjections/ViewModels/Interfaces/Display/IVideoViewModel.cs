using System;
using System.Collections.Generic;
using WallProjections.Models.Interfaces;

namespace WallProjections.ViewModels.Interfaces.Display;

/// <summary>
/// A viewmodel for displaying videos.
/// </summary>
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
    /// Whether the player should be shown.
    /// </summary>
    public bool IsVisible { get; }

    /// <summary>
    /// The volume of the video.
    /// </summary>
    public int Volume { get; set; }

    /// <summary>
    /// Marks the video player as properly loaded.
    /// </summary>
    public void MarkLoaded();

    /// <summary>
    /// Plays the video at the given path.
    /// </summary>
    /// <param name="path">The path to the video.</param>
    /// <returns>Whether or not the video has successfully started playing.</returns>
    public bool PlayVideo(string path) => PlayVideos(new[] { path });

    /// <summary>
    /// Plays the videos in order at the given paths.
    /// </summary>
    /// <param name="paths">The paths to the videos.</param>
    /// <returns>Whether or not the videos have successfully started playing.</returns>
    public bool PlayVideos(IEnumerable<string> paths);

    /// <summary>
    /// Plays the next video in the queue.
    /// </summary>
    /// <returns>Whether the video has started playing successfully.</returns>
    public bool PlayNextVideo();

    /// <summary>
    /// Stops the currently playing video.
    /// </summary>
    public void StopVideo();
}
