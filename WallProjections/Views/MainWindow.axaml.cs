using Avalonia.Input;
using Avalonia.ReactiveUI;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.Views;

public partial class MainWindow : ReactiveWindow<IMainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // ReSharper disable UnusedParameter.Local
    //TODO Get rid of this
    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        var key = e.Key switch
        {
            Key.D1 => "1",
            Key.D2 => "2",
            Key.D3 => "3",
            _ => null
        };

        if (key is not null)
            ViewModel?.CreateDisplayViewModel(key);
    }
}
