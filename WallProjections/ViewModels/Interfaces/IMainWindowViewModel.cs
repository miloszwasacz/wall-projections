namespace WallProjections.ViewModels.Interfaces;

public interface IMainWindowViewModel
{
    public IDisplayViewModel? DisplayViewModel { get; }
    public void CreateDisplayViewModel(string id);
}
