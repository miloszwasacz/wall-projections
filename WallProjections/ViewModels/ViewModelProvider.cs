using System;
using LibVLCSharp.Shared;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
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
    /// The backing field for the <see cref="LibVlc"/> property
    /// </summary>
    /// <remarks>Reset when <see cref="Dispose"/> is called so that a new instance can be created if needed</remarks>
    private LibVLC? _libVlc;

    /// <summary>
    /// A global instance of <see cref="LibVLC"/> to use for <see cref="LibVLCSharp"/> library
    /// </summary>
    /// <remarks>Only instantiated if needed</remarks>
    private LibVLC LibVlc => _libVlc ??= new LibVLC();

    private ViewModelProvider()
    {
    }

    /// <summary>
    /// Creates a new <see cref="MainWindowViewModel"/> instance
    /// </summary>
    /// <returns>A new <see cref="MainWindowViewModel"/> instance</returns>
    public IMainWindowViewModel GetMainWindowViewModel() => new MainWindowViewModel(this);

    //TODO Add Hotspot reference in the docs
    /// <summary>
    /// Creates a new <see cref="DisplayViewModel"/> instance with the given <paramref name="id"/>
    /// </summary>
    /// <param name="id">The id of a Hotspot</param>
    /// <param name="contentProvider">The <see cref="IContentProvider"/> to look for files associated with the hotspot</param>
    /// <returns>A new <see cref="DisplayViewModel"/> instance</returns>
    public IDisplayViewModel GetDisplayViewModel(int id, IContentProvider contentProvider) =>
        new DisplayViewModel(this, contentProvider, id);

    /// <summary>
    /// Creates a new <see cref="VideoViewModel"/> instance with the given <paramref name="videoPath"/>
    /// </summary>
    /// <param name="videoPath">The path to the video to play</param>
    /// <returns>A new <see cref="VideoViewModel"/> instance</returns>
    public IVideoViewModel GetVideoViewModel(string videoPath) =>
        new VideoViewModel(videoPath, LibVlc, new VLCMediaPlayer(LibVlc));

    /// <summary>
    /// Disposes of the <see cref="LibVlc"/> instance on resets the backing field to <i>null</i>,
    /// so that a new instance can be created if needed
    /// </summary>
    public void Dispose()
    {
        _libVlc?.Dispose();
        _libVlc = null;
    }
}
