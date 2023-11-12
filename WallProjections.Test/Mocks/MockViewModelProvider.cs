using WallProjections.ViewModels.Interfaces;

namespace WallProjections.Test.Mocks;

public class MockViewModelProvider : IViewModelProvider
{
    public IMainWindowViewModel GetMainWindowViewModel()
    {
        throw new NotImplementedException();
    }

    public IDisplayViewModel GetDisplayViewModel(string id)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates a new <see cref="MockVideoViewModel"/> with the given <paramref name="videoPath"/>
    /// </summary>
    /// <param name="videoPath">The path to the video the viewmodel is supposed to play</param>
    /// <returns>A new <see cref="MockVideoViewModel"/></returns>
    public IVideoViewModel GetVideoViewModel(string videoPath) => new MockVideoViewModel(videoPath);
}
