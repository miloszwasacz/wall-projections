namespace WallProjections.ViewModels.Interfaces;

public interface IViewModelProvider
{
    /// <summary>
    /// Creates a new <see cref="IMainWindowViewModel"/> instance
    /// </summary>
    /// <returns>A new <see cref="IMainWindowViewModel"/> instance</returns>
    public IMainWindowViewModel GetMainWindowViewModel();

    //TODO Add Hotspot reference in the docs
    /// <summary>
    /// Creates a new <see cref="IDisplayViewModel"/> instance with the given <paramref name="id"/>
    /// </summary>
    /// <param name="id">The id of a Hotspot</param>
    /// <returns>A new <see cref="IDisplayViewModel"/> instance</returns>
    public IDisplayViewModel GetDisplayViewModel(string id);

    /// <summary>
    /// Creates a new <see cref="IVideoViewModel"/> instance with the given <paramref name="videoPath"/>
    /// </summary>
    /// <param name="videoPath">The path to the video to play</param>
    /// <returns>A new <see cref="IVideoViewModel"/> instance</returns>
    public IVideoViewModel GetVideoViewModel(string videoPath);
}
