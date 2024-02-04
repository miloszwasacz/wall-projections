using System;
using LibVLCSharp.Shared;
using WallProjections.Helper;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.ViewModels;

public sealed class ViewModelProvider : IViewModelProvider, IDisposable
{
    /// <summary>
    /// The backing field for the <see cref="ViewModelProvider" /> property
    /// </summary>
    private static ViewModelProvider? _viewModelProvider;

    /// <summary>
    /// The backing field for the <see cref="LibVlc" /> property
    /// </summary>
    /// <remarks>Reset when <see cref="Dispose" /> is called so that a new instance can be created if needed</remarks>
    private LibVLC? _libVlc;

    private ViewModelProvider()
    {
    }

    /// <summary>
    /// A global instance of <see cref="ViewModelProvider" />
    /// </summary>
    public static ViewModelProvider Instance => _viewModelProvider ??= new ViewModelProvider();

    private DisplayViewModel? _displayViewModel;

    /// <summary>
    /// A global instance of <see cref="LibVLC" /> to use for <see cref="LibVLCSharp" /> library
    /// </summary>
    /// <remarks>Only instantiated if needed</remarks>
    private LibVLC LibVlc => _libVlc ??= new LibVLC();

    /// TODO: Maybe improve the way the <see cref="DisplayViewModel"/> is cached.
    /// <summary>
    /// Creates a new <see cref="DisplayViewModel" /> instance
    /// </summary>
    /// <param name="config">The <see cref="IConfig" /> containing data about the hotspots</param>
    /// <returns>A new <see cref="DisplayViewModel" /> instance</returns>
    public IDisplayViewModel GetDisplayViewModel(IConfig config) =>
        _displayViewModel ??= new DisplayViewModel(this, config, PythonEventHandler.Instance);

    /// <summary>
    /// Creates a new <see cref="ImageViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="ImageViewModel" /> instance</returns>
    public IImageViewModel GetImageViewModel() => new ImageViewModel();

    /// <summary>
    /// Creates a new <see cref="VideoViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="VideoViewModel" /> instance</returns>
    public IVideoViewModel GetVideoViewModel() => new VideoViewModel(LibVlc, new VLCMediaPlayer(LibVlc));

    /// <summary>
    /// Disposes of the <see cref="LibVlc" /> instance on resets the backing field to <i>null</i>,
    /// so that a new instance can be created if needed
    /// </summary>
    public void Dispose()
    {
        _libVlc?.Dispose();
        _libVlc = null;
    }
}
