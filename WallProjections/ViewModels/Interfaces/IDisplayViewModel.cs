namespace WallProjections.ViewModels.Interfaces;

public interface IDisplayViewModel
{
    public string Description { get; }
    public IVideoViewModel? VideoViewModel { get; }

    //TODO Change to an interface
    public ViewModelBase? ImageViewModel { get; }

    public bool HasImages { get; }
    public bool HasVideos { get; }
}
