using ReactiveUI;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.ViewModels;

public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    private readonly IViewModelProvider _vmProvider;
    private IDisplayViewModel? _displayViewModel;

    public IDisplayViewModel? DisplayViewModel
    {
        get => _displayViewModel;
        private set => this.RaiseAndSetIfChanged(ref _displayViewModel, value);
    }

    public MainWindowViewModel(IViewModelProvider vmProvider)
    {
        _vmProvider = vmProvider;
    }

    public void CreateDisplayViewModel(int id, IContentProvider contentProvider)
    {
        DisplayViewModel = _vmProvider.GetDisplayViewModel(id, contentProvider);
    }

    //TODO Handle Python events (see f1dd495)
}
