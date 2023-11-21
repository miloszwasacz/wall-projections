using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.Views;

public partial class DisplayWindow : ReactiveWindow<IDisplayViewModel>
{
    private IConfig? _config;

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

        var vm = ViewModel;
        if (vm is null) return;

        Task.Run(async () =>
        {
            while (_config is null)
            {
                var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    AllowMultiple = false
                });
                if (files.Count == 0) continue;

                var zipPath = files[0].Path.AbsolutePath;
                if (!zipPath.EndsWith(".zip")) continue;

                _config = ContentCache.Instance.Load(zipPath);
            }

            vm.Config = _config;
            vm.LoadHotspot(key.Value);
        });
#endif
    }

    internal void OnVideoViewResize(object? sender, SizeChangedEventArgs e)
    {
        if (e.WidthChanged)
            //TODO Don't use hardcoded ratio
            VideoView.Height = e.NewSize.Width * 9 / 16;
    }
}
