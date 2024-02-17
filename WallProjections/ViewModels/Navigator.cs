using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;
using WallProjections.Views;
using AppLifetime = Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;

namespace WallProjections.ViewModels;

/// <inheritdoc cref="INavigator" />
public sealed class Navigator : ViewModelBase, INavigator, IDisposable
{
    /// <summary>
    /// A mutex ensure sequential access to the main window.
    /// </summary>
    private readonly Mutex _windowMutex = new();

    /// <summary>
    /// The application lifetime.
    /// </summary>
    private readonly AppLifetime _appLifetime;

    /// <summary>
    /// The viewmodel provider.
    /// </summary>
    private readonly IViewModelProvider _vmProvider;

    /// <summary>
    /// A factory for creating <see cref="IFileHandler">file handlers</see>.
    /// </summary>
    private readonly Func<IFileHandler> _fileHandlerFactory;

    /// <summary>
    /// Currently loaded configuration.
    /// </summary>
    private IConfig? _config;

    /// <summary>
    /// Creates a new instance of <see cref="Navigator" />.
    /// </summary>
    /// <param name="appLifetime">The application lifetime.</param>
    /// <param name="vmProviderFactory">A factory to create a <see cref="IViewModelProvider" /> instance.</param>
    /// <param name="fileHandlerFactory">A factory to create <see cref="IFileHandler">file handlers</see>.</param>
    public Navigator(
        AppLifetime appLifetime,
        Func<INavigator, IViewModelProvider> vmProviderFactory,
        Func<IFileHandler> fileHandlerFactory
    )
    {
        _appLifetime = appLifetime;
        _fileHandlerFactory = fileHandlerFactory;
        _vmProvider = vmProviderFactory(this);
        _appLifetime.Exit += OnExit;

        Initialize();
    }

    /// <summary>
    /// Sets up the initial state of the application -- if a configuration file is found, the display window is opened,
    /// otherwise the editor window is opened.
    /// </summary>
    private void Initialize()
    {
        var fileHandler = _fileHandlerFactory();
        try
        {
            _config = fileHandler.LoadConfig();
            OpenDisplay(true);
        }
        catch (Exception e)
        {
            //TODO Log to file
            Console.Error.WriteLine(e);
            OpenEditor();
        }
    }

    /// <summary>
    /// Opens the <see cref="DisplayWindow" /> with the current configuration.
    /// If no configuration is found, the <see cref="OpenEditor">editor window</see> is opened
    /// (if <paramref name="openEditorOnFail" /> is <i>true</i>).
    /// </summary>
    /// <param name="openEditorOnFail">Whether to open the editor window if the configuration is not found.</param>
    private void OpenDisplay(bool openEditorOnFail)
    {
        _windowMutex.WaitOne();
        var config = _config;
        if (config == null)
        {
            _windowMutex.ReleaseMutex();
            if (openEditorOnFail)
                OpenEditor();
            return;
        }

        Navigate(new DisplayWindow
        {
            DataContext = _vmProvider.GetDisplayViewModel(config)
        });
        _windowMutex.ReleaseMutex();
    }

    /// <summary>
    /// Opens the <see cref="EditorWindow" />.
    /// </summary>
    public void OpenEditor()
    {
        _windowMutex.WaitOne();
        var config = _config;
        var fileHandler = _fileHandlerFactory();

        // Based on whether a configuration is found, the editor viewmodel
        // is created with or without a base configuration.
        var vm = config is not null
            ? _vmProvider.GetEditorViewModel(config, fileHandler)
            : _vmProvider.GetEditorViewModel(fileHandler);

        Navigate(new EditorWindow
        {
            DataContext = vm
        });
        _windowMutex.ReleaseMutex();
    }

    /// <summary>
    /// Closes the <see cref="OpenEditor">Editor</see> and opens the <see cref="OpenDisplay">Display</see> window.
    /// If no configuration is found, the application is closed.
    /// If an error occurs, a new <see cref="OpenEditor">Editor</see> is opened.
    /// </summary>
    public void CloseEditor()
    {
        var fileHandler = _fileHandlerFactory();
        try
        {
            _config = fileHandler.LoadConfig();
            OpenDisplay(false);
        }
        catch (Exception e)
        {
            //TODO Log to file
            Console.Error.WriteLine(e);
            OpenEditor();
        }
    }

    /// <summary>
    /// Shuts down the application.
    /// </summary>
    /// <seealso cref="AppLifetime.Shutdown(int)"/>
    public void Shutdown()
    {
        _appLifetime.Shutdown();
    }

    /// <summary>
    /// Opens the specified window, sets it as the <see cref="AppLifetime.MainWindow" />,
    /// and <see cref="WindowExtensions.CloseAndDispose">closes</see> the currently opened window.
    /// </summary>
    /// <param name="newWindow">The new window to open.</param>
    private void Navigate(Window newWindow)
    {
        var currentWindow = _appLifetime.MainWindow;
        newWindow.Show();
        _appLifetime.MainWindow = newWindow;
        currentWindow?.CloseAndDispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_vmProvider is IDisposable vmProvider)
            vmProvider.Dispose();

        _appLifetime.MainWindow?.CloseAndDispose();
        _appLifetime.Exit -= OnExit;
    }

    // ReSharper disable UnusedParameter.Local

    /// <summary>
    /// A callback for the <see cref="_appLifetime" />'s <see cref="AppLifetime.Exit" /> event
    /// which simply <see cref="Dispose">disposes</see> the <see cref="Navigator" />.
    /// </summary>
    private void OnExit(object? sender, EventArgs e)
    {
        Dispose();
    }

    // ReSharper restore UnusedParameter.Local
}

/// <summary>
/// Extension methods for <see cref="Window" />.
/// </summary>
internal static class WindowExtensions
{
    /// <summary>
    /// Disposes <paramref name="window" />'s <see cref="Window.DataContext" />
    /// and <see cref="Window.Close()">closes</see> the <paramref name="window" />.
    /// </summary>
    /// <param name="window">The window to close and dispose.</param>
    public static async void CloseAndDispose(this Window window)
    {
        var dataContext = window.DataContext;
        window.ShowInTaskbar = false;

        if (dataContext is IDisposable vm)
            vm.Dispose();

        // A delay to allow a smooth transition between windows.
        await Task.Delay(200);
        window.Close();
    }
}
