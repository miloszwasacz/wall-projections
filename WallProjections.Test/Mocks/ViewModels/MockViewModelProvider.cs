using WallProjections.ViewModels;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.Test.Mocks.ViewModels;

public class MockViewModelProvider : IViewModelProvider
{
    /// <summary>
    /// Creates a new <see cref="MockDisplayViewModel"/>
    /// </summary>
    /// <returns>A new <see cref="MockDisplayViewModel"/></returns>
    public IDisplayViewModel GetDisplayViewModel() => new MockDisplayViewModel();

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
    
    /// <summary>
    /// Creates a new <see cref="MockHotspotViewModel"/>
    /// </summary>
    /// <returns>A new <see cref="MockHotspotViewModel"/></returns>
    public IHotspotViewModel GetHotspotViewModel() => new MockHotspotViewModel();
}
