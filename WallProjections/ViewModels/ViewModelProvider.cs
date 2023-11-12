using System;
using LibVLCSharp.Shared;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.ViewModels;

public sealed class ViewModelProvider : IViewModelProvider, IDisposable
{
    /// <summary>
    /// The backing field for the <see cref="ViewModelProvider"/> property
    /// </summary>
    private static ViewModelProvider? _viewModelProvider;

    /// <summary>
    /// A global instance of <see cref="ViewModelProvider"/>
    /// </summary>
    public static ViewModelProvider Instance => _viewModelProvider ??= new ViewModelProvider();

    /// <summary>
    /// A global instance of <see cref="LibVLC"/> to use for <see cref="LibVLCSharp"/> library
    /// </summary>
    private readonly LibVLC _libVlc = new();

    private ViewModelProvider()
    {
    }

    /// <summary>
    /// Creates a new <see cref="MainWindowViewModel"/> instance
    /// </summary>
    /// <returns>A new <see cref="MainWindowViewModel"/> instance</returns>
    public IMainWindowViewModel GetMainWindowViewModel() =>
        new MainWindowViewModel(this);

    //TODO Add Hotspot reference in the docs
    /// <summary>
    /// Creates a new <see cref="DisplayViewModel"/> instance with the given <paramref name="id"/>
    /// </summary>
    /// <param name="id">The id of a Hotspot</param>
    /// <returns>A new <see cref="DisplayViewModel"/> instance</returns>
    public IDisplayViewModel GetDisplayViewModel(string id) =>
        new DisplayViewModel(this, new FileProvider(), id);

    /// <summary>
    /// Creates a new <see cref="VideoViewModel"/> instance with the given <paramref name="videoPath"/>
    /// </summary>
    /// <param name="videoPath">The path to the video to play</param>
    /// <returns>A new <see cref="VideoViewModel"/> instance</returns>
    public IVideoViewModel GetVideoViewModel(string videoPath) =>
        new VideoViewModel(videoPath, _libVlc, new VLCMediaPlayer(_libVlc));

    public void Dispose()
    {
        _libVlc.Dispose();
    }
}
