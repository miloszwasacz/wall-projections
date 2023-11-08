using ReactiveUI;

namespace WallProjections.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private DisplayViewModel? _displayViewModel;

    public DisplayViewModel? DisplayViewModel
    {
        get => _displayViewModel;
        set => this.RaiseAndSetIfChanged(ref _displayViewModel, value);
    }

    public void CreateDisplayViewModel(string fileNumber)
    {
        DisplayViewModel = new DisplayViewModel(fileNumber);
    }
}
