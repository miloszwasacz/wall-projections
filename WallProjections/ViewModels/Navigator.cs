using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using WallProjections.Helper.Interfaces;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;
using WallProjections.Views;
using WallProjections.Views.SecondaryScreens;
using AppLifetime = Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;

namespace WallProjections.ViewModels;

/// <inheritdoc cref="INavigator" />
public sealed class Navigator : ViewModelBase, INavigator
{
    /// <summary>
    /// A mutex to ensure sequential access to <see cref="_appLifetime" />.<see cref="AppLifetime.MainWindow" />
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
    /// The handler for Python interop.
    /// </summary>
    private readonly IPythonHandler _pythonHandler;

    /// <summary>
    /// A factory for creating <see cref="IFileHandler">file handlers</see>.
    /// </summary>
    private readonly Func<IFileHandler> _fileHandlerFactory;

    /// <summary>
    /// Currently loaded configuration.
    /// </summary>
    private IConfig? _config;

    /// <summary>
    /// Currently opened main window
    /// </summary>
    private Window? _mainWindow;

    /// <summary>
    /// Secondary screen window and its viewmodel
    /// </summary>
    private readonly (Window window, ISecondaryWindowViewModel viewModel) _secondaryScreen;

    /// <summary>
    /// Creates a new instance of <see cref="Navigator" />.
    /// </summary>
    /// <param name="appLifetime">The application lifetime.</param>
    /// <param name="pythonHandler">The handler for Python interop.</param>
    /// <param name="vmProviderFactory">A factory to create a <see cref="IViewModelProvider" /> instance.</param>
    /// <param name="fileHandlerFactory">A factory to create <see cref="IFileHandler">file handlers</see>.</param>
    public Navigator(
        AppLifetime appLifetime,
        IPythonHandler pythonHandler,
        Func<INavigator, IPythonHandler, IViewModelProvider> vmProviderFactory,
        Func<IFileHandler> fileHandlerFactory
    )
    {
        _appLifetime = appLifetime;
        _pythonHandler = pythonHandler;
        _fileHandlerFactory = fileHandlerFactory;
        _vmProvider = vmProviderFactory(this, pythonHandler);
        _secondaryScreen = OpenSecondaryWindow(_vmProvider);
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
            OpenDisplay();
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
    /// If no configuration is found, the <see cref="OpenEditor">editor window</see> is opened.
    /// </summary>
    private void OpenDisplay()
    {
        _windowMutex.WaitOne();
        var config = _config;
        if (config == null)
        {
            _windowMutex.ReleaseMutex();
            OpenEditor();
            return;
        }

        _pythonHandler.RunHotspotDetection();
        var displayWindow = new DisplayWindow
        {
            DataContext = _vmProvider.GetDisplayViewModel(config)
        };
        Navigate(displayWindow);
        _secondaryScreen.viewModel.ShowHotspotDisplay(config);

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

        _pythonHandler.CancelCurrentTask();
        var editorWindow = new EditorWindow
        {
            DataContext = vm
        };
        Navigate(editorWindow);
        _secondaryScreen.viewModel.ShowPositionEditor();

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
            OpenDisplay();
        }
        catch (Exception e)
        {
            //TODO Log to file
            Console.Error.WriteLine(e);
            _config = null;
            //TODO Show error message
            Shutdown();
        }
    }

    /// <inheritdoc />
    public void ShowCalibrationMarkers()
    {
        _secondaryScreen.viewModel.ShowArUcoGrid();
    }

    /// <inheritdoc />
    public void HideCalibrationMarkers()
    {
        _secondaryScreen.viewModel.ShowPositionEditor();
    }

    /// <inheritdoc />
    public ImmutableDictionary<int, Point>? GetArUcoPositions()
    {
        var arUcoGridView = _secondaryScreen.window.FindDescendantOfType<ArUcoGridView>();
        ImmutableDictionary<int, Point>? positions = null;
        Dispatcher.UIThread.Invoke(() =>
        {
            // This has to be run on the UI thread
            positions = arUcoGridView?.GetArUcoPositions();
        });
        return positions;
    }

    /// <summary>
    /// Shuts down the application.
    /// </summary>
    /// <seealso cref="AppLifetime.Shutdown(int)"/>
    public void Shutdown()
    {
        _secondaryScreen.window.Close();
        _appLifetime.Shutdown();
    }

    /// <summary>
    /// Opens the specified window, sets it as the <see cref="_mainWindow" /> and the <see cref="AppLifetime.MainWindow" />,
    /// and <see cref="WindowExtensions.CloseAndDispose">closes</see> the currently opened window.
    /// </summary>
    /// <param name="newWindow">The new window to open.</param>
    private void Navigate(Window newWindow)
    {
        var currentWindow = _mainWindow;
        _mainWindow = newWindow;
        newWindow.Show();
        _appLifetime.MainWindow = newWindow;
        currentWindow?.CloseAndDispose();
    }

    /// <summary>
    /// Opens a <see cref="SecondaryWindow" /> on the secondary screen (if available).
    /// </summary>
    /// <param name="vmProvider">
    /// The <see cref="IViewModelProvider" /> for creating the <see cref="ISecondaryWindowViewModel" />
    /// </param>
    /// <returns>The opened window and its viewmodel</returns>
    [ExcludeFromCodeCoverage(Justification = "Headless mode doesn't support multiple screens")]
    private static (SecondaryWindow, ISecondaryWindowViewModel) OpenSecondaryWindow(IViewModelProvider vmProvider)
    {
        var vm = vmProvider.GetSecondaryWindowViewModel();
        var window = new SecondaryWindow
        {
            DataContext = vm
        };

        var screens = window.Screens;
        var secondaryScreen = screens.All.FirstOrDefault(s => !s.IsPrimary);
        if (secondaryScreen is not null)
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Position = secondaryScreen.Bounds.Position;
            window.Show();
            window.WindowState = WindowState.FullScreen;
        }
        else
            window.Show();

        return (window, vm);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_vmProvider is IDisposable vmProvider)
            vmProvider.Dispose();

        if (_appLifetime.MainWindow?.DataContext is IDisposable vm)
            vm.Dispose();

        _pythonHandler.CancelCurrentTask();

        _appLifetime.MainWindow = null;
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
