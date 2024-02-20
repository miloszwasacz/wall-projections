using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using WallProjections.ViewModels;
using WallProjections.Views;

namespace WallProjections;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var displayViewModel = ViewModelProvider.Instance.GetDisplayViewModel();
            desktop.MainWindow = new DisplayWindow
            {
                DataContext = displayViewModel
            };
            var w = new HotspotDisplayWindow
            {
                DataContext = displayViewModel.HotspotViewModel
            };
            w.Show();
            

            desktop.Exit += OnExit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;

        if (desktop.MainWindow?.DataContext is IDisposable vm)
            vm.Dispose();
    }
}
