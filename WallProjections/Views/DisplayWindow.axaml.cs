using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using WallProjections.ViewModels.Interfaces;
using Avalonia.Platform.Storage;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using System.Diagnostics.CodeAnalysis;
#if DEBUGSKIPPYTHON
using Avalonia.Data;
using WallProjections.Helper;
#endif

namespace WallProjections.Views;

public partial class DisplayWindow : ReactiveWindow<IDisplayViewModel>
{
    [ExcludeFromCodeCoverage] private IConfig? Config { get; set; }

    public DisplayWindow()
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
                return;
            case Key.F11:
                WindowState = WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;
                return;
        }

        var vm = ViewModel;
        if (vm is null) return;

        LoadConfig(vm, e.Key);
    }

    internal void OnVideoViewResize(object? sender, SizeChangedEventArgs e)
    {
        if (e.WidthChanged)
            //TODO Don't use hardcoded ratio
            VideoView.Height = e.NewSize.Width * 9 / 16;
    }

    //TODO Get rid of this - figure out how to set the config in the viewmodel
    [ExcludeFromCodeCoverage]
    private async void LoadConfig(IDisplayViewModel vm, Key key)
    {
        while (Config is null)
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false
            });
            if (files.Count == 0) continue;

            var zipPath = files[0].Path.AbsolutePath;
            if (!zipPath.EndsWith(".zip")) continue;

            Config = ContentCache.Instance.Load(zipPath);
        }

        vm.Config = Config;
#if DEBUGSKIPPYTHON
        MockPythonInput(key);
#endif
    }

#if DEBUGSKIPPYTHON
    [ExcludeFromCodeCoverage]
    private static void MockPythonInput(Key key)
    {
        var keyVal = key switch
        {
            Key.D1 => new Optional<int>(1),
            Key.D2 => new Optional<int>(2),
            Key.D3 => new Optional<int>(3),
            Key.D4 => new Optional<int>(4),
            Key.D5 => new Optional<int>(5),
            _ => new Optional<int>()
        };

        if (!keyVal.HasValue) return;

        PythonEventHandler.Instance.OnPressDetected(keyVal.Value);
    }
#endif
}
