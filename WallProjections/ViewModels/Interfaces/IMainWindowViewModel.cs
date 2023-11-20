using WallProjections.Models.Interfaces;

namespace WallProjections.ViewModels.Interfaces;

public interface IMainWindowViewModel
{
    public IDisplayViewModel? DisplayViewModel { get; }
    public void CreateDisplayViewModel(int id, IContentProvider contentProvider);
}
