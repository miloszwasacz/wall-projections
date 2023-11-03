using ReactiveUI;
using WallProjections.Helper;

namespace WallProjections.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string _greeting = "Welcome to Avalonia!";

    public string Greeting
    {
        get => _greeting;
        private set => this.RaiseAndSetIfChanged(ref _greeting, value);
    }

    public MainWindowViewModel()
    {
        PythonEventHandler.Instance.PressDetected += (_, args) =>
        {
            Greeting = args.Button.ToString();
        };
    }
}
