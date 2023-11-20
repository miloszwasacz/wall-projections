﻿using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.Test.Mocks.ViewModels;

public class MockViewModelProvider : IViewModelProvider
{
    /// <summary>
    /// Creates a new <see cref="MockMainWindowViewModel"/> with self as the <see cref="IViewModelProvider"/>
    /// </summary>
    /// <returns>A new <see cref="MockMainWindowViewModel"/></returns>
    public IMainWindowViewModel GetMainWindowViewModel() => new MockMainWindowViewModel(this);

    /// <summary>
    /// Creates a new <see cref="MockDisplayViewModel"/> with the given <paramref name="id"/>
    /// </summary>
    /// <param name="id">The ID of a hotspot the viewmodel should display data about</param>
    /// <param name="contentProvider">
    /// The <see cref="IContentProvider"/> to look for files associated with the hotspot (not used in this mock)
    /// </param>
    /// <returns>A new <see cref="MockDisplayViewModel"/></returns>
    public IDisplayViewModel GetDisplayViewModel(int id, IContentProvider contentProvider) => new MockDisplayViewModel(id);

    /// <summary>
    /// Creates a new <see cref="MockVideoViewModel"/> with the given <paramref name="videoPath"/>
    /// </summary>
    /// <param name="videoPath">The path to the video the viewmodel is supposed to play</param>
    /// <returns>A new <see cref="MockVideoViewModel"/></returns>
    public IVideoViewModel GetVideoViewModel(string videoPath) => new MockVideoViewModel(videoPath);
}
