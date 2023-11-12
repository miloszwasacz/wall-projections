using ReactiveUI;
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

    public void CreateDisplayViewModel(string id)
    {
        DisplayViewModel = _vmProvider.GetDisplayViewModel(id);
    }
}
