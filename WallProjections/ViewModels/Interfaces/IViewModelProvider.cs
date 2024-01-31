using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;

namespace WallProjections.ViewModels.Interfaces;

public interface IViewModelProvider
{
    /// <summary>
    /// Creates a new <see cref="IDisplayViewModel" /> instance
    /// </summary>
    /// <param name="config">The <see cref="IConfig" /> containing data about the hotspots</param>
    /// <returns>A new <see cref="IDisplayViewModel" /> instance</returns>
    public IDisplayViewModel GetDisplayViewModel(IConfig config);

    /// <summary>
    /// Creates a new <see cref="IImageViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="IImageViewModel" /> instance</returns>
    public IImageViewModel GetImageViewModel();

    /// <summary>
    /// Creates a new <see cref="IVideoViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="IVideoViewModel" /> instance</returns>
    public IVideoViewModel GetVideoViewModel();
}
