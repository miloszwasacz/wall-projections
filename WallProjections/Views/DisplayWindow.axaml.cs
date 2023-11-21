using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using WallProjections.ViewModels.Interfaces;
#if DEBUGSKIPPYTHON
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia.Data;
using Avalonia.Platform.Storage;
using WallProjections.Helper;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
#endif

namespace WallProjections.Views;

public partial class DisplayWindow : ReactiveWindow<IDisplayViewModel>
{
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
        MockPythonInput(e.Key);
#endif
    }

    internal void OnVideoViewResize(object? sender, SizeChangedEventArgs e)
    {
        if (e.WidthChanged)
            //TODO Don't use hardcoded ratio
            VideoView.Height = e.NewSize.Width * 9 / 16;
    }

#if DEBUGSKIPPYTHON
    [ExcludeFromCodeCoverage] private IConfig? Config { get; set; }

    [ExcludeFromCodeCoverage]
    private void MockPythonInput(Key key)
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

        var vm = ViewModel;
        if (vm is null) return;

        Task.Run(async () =>
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
            PythonEventHandler.Instance.OnPressDetected(keyVal.Value);
        });
    }
#endif
}
