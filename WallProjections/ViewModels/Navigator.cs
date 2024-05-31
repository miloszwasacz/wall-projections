using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Microsoft.Extensions.Logging;
using WallProjections.Helper.Interfaces;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;
using WallProjections.Views.Display;
using WallProjections.Views.Editor;
using WallProjections.Views.SecondaryScreens;
using AppLifetime = Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;

namespace WallProjections.ViewModels;

/// <inheritdoc cref="INavigator" />
public sealed class Navigator : ViewModelBase, INavigator
{
    /// <summary>
    /// A logger for this class.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// The application lifetime.
    /// </summary>
    private readonly AppLifetime _appLifetime;

    /// <summary>
    /// A method executed when the navigator is shut down (usually because of an error).
    /// </summary>
    private readonly Action<ExitCode> _shutdown;

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
    private Window? MainWindow
    {
        get => _appLifetime.MainWindow;
        set => _appLifetime.MainWindow = value;
    }

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
    /// <param name="shutdown">A method to shut down the application.</param>
    /// <param name="loggerFactory">A factory to create loggers.</param>
    public Navigator(
        AppLifetime appLifetime,
        IPythonHandler pythonHandler,
        Func<INavigator, IPythonHandler, IViewModelProvider> vmProviderFactory,
        Func<IFileHandler> fileHandlerFactory,
        Action<ExitCode> shutdown,
        ILoggerFactory loggerFactory
    )
    {
        _logger = loggerFactory.CreateLogger<Navigator>();
        _appLifetime = appLifetime;
        _shutdown = shutdown;
        _pythonHandler = pythonHandler;
        _fileHandlerFactory = fileHandlerFactory;
        _vmProvider = vmProviderFactory(this, pythonHandler);

        var vm = _vmProvider.GetSecondaryWindowViewModel();
        var window = new SecondaryWindow
        {
            DataContext = vm
        };
        _secondaryScreen = (window, vm);

        Initialize();
    }

    /// <summary>
    /// Sets up the initial state of the application -- if a configuration file is found, the display window is opened,
    /// otherwise the editor window is opened.
    /// </summary>
    private void Initialize()
    {
        try
        {
            var fileHandler = _fileHandlerFactory();
            _config = fileHandler.LoadConfig();
            OpenDisplay();
        }
        catch (ConfigNotImportedException)
        {
            _logger.LogInformation("No configuration found");
            Shutdown(ExitCode.ConfigNotFound);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error loading configuration file");
            Shutdown(ExitCode.ConfigLoadError);
        }
    }

    /// <summary>
    /// Opens the <see cref="DisplayWindow" /> with the current configuration.
    /// If no configuration is found, the <see cref="OpenEditor">editor window</see> is opened.
    /// </summary>
    private void OpenDisplay()
    {
        lock (this)
        {
            OpenDisplayInternal();
        }
    }

    /// <summary>
    /// Same as <see cref="OpenDisplay" /> but without the lock.
    /// </summary>
    private void OpenDisplayInternal()
    {
        lock (this)
        {
            var config = _config;
            if (config is null)
            {
                OpenEditorInternal();
                return;
            }

            // TODO Await the task and handle exceptions
            _pythonHandler.RunHotspotDetection(config);

            var displayWindow = new DisplayWindow
            {
                DataContext = _vmProvider.GetDisplayViewModel(config)
            };
            Navigate(displayWindow, true);
            _secondaryScreen.viewModel.ShowHotspotDisplay(config);
        }
    }

    /// <summary>
    /// Opens the <see cref="EditorWindow" />.
    /// </summary>
    public void OpenEditor()
    {
        lock (this)
        {
            OpenEditorInternal();
        }
    }

    /// <summary>
    /// Same as <see cref="OpenEditor" /> but without the lock.
    /// </summary>
    private void OpenEditorInternal()
    {
        var config = _config;
        var fileHandler = _fileHandlerFactory();

        // Based on whether a configuration is found, the editor viewmodel
        // is created with or without a base configuration.
        var vm = config is not null
            ? _vmProvider.GetEditorViewModel(config, fileHandler)
            : _vmProvider.GetEditorViewModel(fileHandler);

        try
        {
            _pythonHandler.CancelCurrentTask();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error cancelling current Python task");
            Shutdown(ExitCode.PythonError);
            return;
        }

        var editorWindow = new EditorWindow
        {
            DataContext = vm
        };
        Navigate(editorWindow, true);
        _secondaryScreen.viewModel.ShowPositionEditor(vm);
    }

    /// <summary>
    /// Closes the <see cref="OpenEditor">Editor</see> and opens the <see cref="OpenDisplay">Display</see> window.
    /// If no configuration is found, the application is closed.
    /// If an error occurs, a new <see cref="OpenEditor">Editor</see> is opened.
    /// </summary>
    public void CloseEditor()
    {
        lock (this)
        {
            // If _config is null, it means that there was no configuration to begin with.
            var isInitial = _config is null;
            try
            {
                var fileHandler = _fileHandlerFactory();
                _config = fileHandler.LoadConfig();
                OpenDisplayInternal();
            }
            catch (ConfigNotImportedException e)
            {
                // If there was no configuration in the first place, closing the editor is the same as closing the app.
                if (isInitial)
                {
                    _logger.LogInformation("Configuration not initialized and not created, closing application");
                    Shutdown();
                    return;
                }

                _logger.LogError(e, "No configuration found");
                Shutdown(ExitCode.ConfigLoadError);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error loading configuration file");
                _config = null;
                Shutdown(ExitCode.ConfigLoadError);
            }
        }
    }

    /// <inheritdoc />
    public void ShowCalibrationMarkers()
    {
        lock (this)
        {
            _secondaryScreen.viewModel.ShowArUcoGrid();
        }
    }

    /// <inheritdoc />
    public void HideCalibrationMarkers()
    {
        lock (this)
        {
            if (MainWindow?.DataContext is IEditorViewModel editorViewModel)
                _secondaryScreen.viewModel.ShowPositionEditor(editorViewModel);
        }
    }

    /// <inheritdoc />
    public ImmutableDictionary<int, Point>? GetArUcoPositions()
    {
        lock (this)
        {
            var arUcoGridView = _secondaryScreen.window.FindDescendantOfType<ArUcoGridView>();
            return arUcoGridView?.GetArUcoPositions();
        }
    }

    /// <inheritdoc />
    public void Shutdown(ExitCode exitCode = ExitCode.Success)
    {
        var main = MainWindow;
        var secondary = _secondaryScreen.window;

        secondary.ShowInTaskbar = false;
        secondary.Hide();

        if (main is not null)
        {
            main.ShowInTaskbar = false;
            main.Hide();
        }

        _shutdown(exitCode);
    }

    /// <summary>
    /// Opens the specified window, sets it as the <see cref="MainWindow" /> and the <see cref="AppLifetime.MainWindow" />,
    /// and <see cref="WindowExtensions.CloseAndDispose">closes</see> the currently opened window.
    /// </summary>
    /// <param name="newWindow">The new window to open.</param>
    /// <param name="showSecondary">Whether to show the secondary screen.</param>
    private void Navigate(Window newWindow, bool showSecondary)
    {
        lock (this)
        {
            var currentWindow = MainWindow;

            newWindow.Show();
            MainWindow = newWindow;

            if (showSecondary) OpenSecondaryWindow();
            else
            {
                _secondaryScreen.window.ShowInTaskbar = false;
                _secondaryScreen.window.Hide();
            }

            newWindow.Activate();

            currentWindow?.CloseAndDispose();
        }
    }

    /// <summary>
    /// Shows the <see cref="_secondaryScreen">secondary window</see> on the secondary screen (if available).
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Headless mode doesn't support multiple screens")]
    private void OpenSecondaryWindow()
    {
        var window = _secondaryScreen.window;
        window.ShowInTaskbar = true;

        var screens = window.Screens;
        var secondaryScreen = screens.All.FirstOrDefault(s => !s.IsPrimary);
        if (secondaryScreen is not null)
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Position = secondaryScreen.Bounds.Position;
            window.Show();
            window.WindowState = WindowState.Maximized;
            window.WindowState = WindowState.FullScreen;
        }
        else
            window.Show();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (this)
        {
            var mainWindow = MainWindow;
            MainWindow = null;
            if (_vmProvider is IDisposable vmProvider)
                vmProvider.Dispose();

            if (mainWindow?.DataContext is IDisposable vm)
                vm.Dispose();
        }
    }
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
    public static void CloseAndDispose(this Window window)
    {
        var dataContext = window.DataContext;
        window.ShowInTaskbar = false;

        if (dataContext is IDisposable vm)
            vm.Dispose();

        window.Hide();
        window.Close();
    }
}
