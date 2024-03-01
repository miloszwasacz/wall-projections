using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive;
using Avalonia.Platform;
using ReactiveUI;
using WallProjections.Models.Interfaces;

namespace WallProjections.ViewModels.Interfaces.Display;

public interface IVideoViewModel : IDisposable
{
    /// <summary>
    /// The <see cref="IMediaPlayer" /> used to play videos
    /// </summary>
    public IMediaPlayer? MediaPlayer { get; }

    /// <summary>
    /// Whether or not the viewmodel has a video to display
    /// </summary>
    public bool HasVideos { get; }
    
    /// <summary>
    /// Whether the player should be shown
    /// </summary>
    public bool IsVisible { get; }

    /// <summary>
    /// The volume of the video.
    /// </summary>
    public int Volume { get; set; }

    /// <summary>
    /// Tells the view model the video player is loaded
    /// </summary>
    public void Loaded();

    /// <summary>
    /// Plays the video at the given path
    /// </summary>
    /// <param name="path">The path to the video.</param>
    /// <returns>Whether or not the video has successfully started playing.</returns>
    public bool PlayVideo(string path);

    /// <summary>
    /// Plays the videos in order at the given paths
    /// </summary>
    /// <param name="paths">The paths to the videos</param>
    /// <returns>Whether or not the videos have successfully started playing.</returns>
    public bool PlayVideos(IEnumerable<string> paths);

    /// <summary>
    /// Plays the next video in the queue
    /// </summary>
    /// <returns><i>true</i> if video is played successfully, <i>false</i> otherwise.</returns>
    public bool PlayNextVideo();

    /// <summary>
    /// Stops the currently playing video
    /// </summary>
    public void StopVideo();
}
