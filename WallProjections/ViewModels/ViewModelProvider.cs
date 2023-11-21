using System;
using LibVLCSharp.Shared;
using WallProjections.Models;
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

    /// <summary>
    /// A global instance of <see cref="LibVLC" /> to use for <see cref="LibVLCSharp" /> library
    /// </summary>
    /// <remarks>Only instantiated if needed</remarks>
    private LibVLC LibVlc => _libVlc ??= new LibVLC();

    /// <summary>
    /// Creates a new <see cref="DisplayViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="DisplayViewModel" /> instance</returns>
    public IDisplayViewModel GetDisplayViewModel() => new DisplayViewModel(this);

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
