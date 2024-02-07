using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;

namespace WallProjections.Test.Mocks.ViewModels;

public class MockViewModelProvider : IViewModelProvider
{
    /// <summary>
    /// Creates a new <see cref="MockDisplayViewModel"/>
    /// </summary>
    /// <returns>A new <see cref="MockDisplayViewModel"/></returns>
    public IDisplayViewModel GetDisplayViewModel(IConfig config) => new MockDisplayViewModel();

    /// <summary>
    /// Creates a new <see cref="MockImageViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="MockImageViewModel" /> instance</returns>
    public IImageViewModel GetImageViewModel() => new MockImageViewModel();

    /// <summary>
    /// Creates a new <see cref="MockVideoViewModel"/>
    /// </summary>
    /// <returns>A new <see cref="MockVideoViewModel"/></returns>
    public IVideoViewModel GetVideoViewModel() => new MockVideoViewModel();
}
