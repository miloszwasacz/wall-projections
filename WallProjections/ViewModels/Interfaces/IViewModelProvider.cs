namespace WallProjections.ViewModels.Interfaces;

public interface IViewModelProvider
{
    //TODO Add Hotspot reference in the docs
    /// <summary>
    /// Creates a new <see cref="IDisplayViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="IDisplayViewModel" /> instance</returns>
    public IDisplayViewModel GetDisplayViewModel();

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
