using ReactiveUI;
using WallProjections.Helper;

namespace WallProjections.ViewModels;

public class MainWindowViewModel : ActivatableViewModelBase
{
    private string _greeting = "Welcome to Avalonia!";

    public string Greeting
    {
        get => _greeting;
        private set => this.RaiseAndSetIfChanged(ref _greeting, value);
    }

    private void OnPressDetected(object? _, PythonEventHandler.PressDetectedArgs args)
    {
        Greeting = args.Button.ToString();
    }

    protected override void OnStart()
    {
        PythonEventHandler.Instance.PressDetected += OnPressDetected;
    }

    protected override void OnStop()
    {
        PythonEventHandler.Instance.PressDetected -= OnPressDetected;
    }
}
