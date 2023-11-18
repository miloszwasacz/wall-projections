using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.Views;

public partial class MainWindow : ReactiveWindow<IMainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        WindowState = WindowState.FullScreen;
    }

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                var lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                lifetime?.Shutdown();
                break;
            case Key.F11:
                WindowState = WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;
                break;
        }

#if DEBUGSKIPPYTHON
        var key = e.Key switch
        {
            Key.D1 => "1",
            Key.D2 => "2",
            Key.D3 => "3",
            _ => null
        };

        if (key is not null)
            ViewModel?.CreateDisplayViewModel(key, new FileProvider());
#endif
    }
}
