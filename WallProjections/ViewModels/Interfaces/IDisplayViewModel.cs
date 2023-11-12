namespace WallProjections.ViewModels.Interfaces;

public interface IDisplayViewModel
{
    public string Description { get; }
    public IVideoViewModel? VideoViewModel { get; }
}
