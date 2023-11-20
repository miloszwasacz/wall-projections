using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.Views;

public partial class MainWindow : ReactiveWindow<IMainWindowViewModel>
{
    private readonly IContentCache _cache;
    private readonly IConfig _config;

    public MainWindow()
    {
        InitializeComponent();
        WindowState = WindowState.FullScreen;

        //TODO Get rid of this hardcoded path and ContentCache
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Assets", "data.zip");
        _cache = new ContentCache();
        _config = _cache.Load(path);
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
            Key.D1 => new Optional<int>(1),
            Key.D2 => new Optional<int>(2),
            Key.D3 => new Optional<int>(3),
            Key.D4 => new Optional<int>(4),
            Key.D5 => new Optional<int>(5),
            _ => new Optional<int>()
        };

        if (!key.HasValue) return;

        ViewModel?.CreateDisplayViewModel(key.Value, new ContentProvider(_cache, _config));
#endif
    }
}
