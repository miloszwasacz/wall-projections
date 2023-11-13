﻿using WallProjections.ViewModels.Interfaces;

namespace WallProjections.Test.Mocks;

public class MockViewModelProvider : IViewModelProvider
{
    public IMainWindowViewModel GetMainWindowViewModel()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates a new <see cref="MockDisplayViewModel"/> with the given <paramref name="id"/>
    /// </summary>
    /// <param name="id">The ID of an artifact the viewmodel should display data about</param>
    /// <returns>A new <see cref="MockDisplayViewModel"/></returns>
    public IDisplayViewModel GetDisplayViewModel(string id) => new MockDisplayViewModel(id);

    /// <summary>
    /// Creates a new <see cref="MockVideoViewModel"/> with the given <paramref name="videoPath"/>
    /// </summary>
    /// <param name="videoPath">The path to the video the viewmodel is supposed to play</param>
    /// <returns>A new <see cref="MockVideoViewModel"/></returns>
    public IVideoViewModel GetVideoViewModel(string videoPath) => new MockVideoViewModel(videoPath);
}
