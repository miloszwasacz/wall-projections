using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.Logging;
using WallProjections.Helper;
using WallProjections.Helper.Interfaces;
using WallProjections.Models;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Display.Layouts;
using WallProjections.ViewModels.Interfaces;
using WallProjections.Views;
using static WallProjections.Views.ResultDialog;
using AppLifetime = Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;

namespace WallProjections;

/// <summary>
/// Manages the global application state.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This class manages the application lifetime, which is not testable")]
public class GlobalWindowManager : IDisposable
{
    /// <summary>
    /// The path to the warning icon.
    /// </summary>
    private static readonly Uri WarningIconPath = new("avares://WallProjections/Assets/warning-icon.ico");

    /// <summary>
    /// A factory for creating loggers.
    /// </summary>
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// A logger for this class.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// The application lifetime.
    /// </summary>
    private readonly AppLifetime _appLifetime;

    /// <summary>
    /// A proxy for starting and managing processes.
    /// </summary>
    private readonly IProcessProxy _processProxy;

    /// <summary>
    /// A proxy directly interfacing with Python.
    /// </summary>
    private readonly IPythonProxy _pythonProxy;

    /// <summary>
    /// A handler for Python interop.
    /// </summary>
    private IPythonHandler? _pythonHandler;

    /// <summary>
    /// A navigator for application-wide navigation between views.
    /// </summary>
    private INavigator? _navigator;

    /// <inheritdoc cref="_pythonHandler" />
    /// <exception cref="InvalidOperationException">If PythonHandler is not initialized.</exception>
    public IPythonHandler PythonHandler =>
        _pythonHandler ?? throw new InvalidOperationException("PythonHandler is not initialized");

    /// <summary>
    /// Whether the object has been disposed.
    /// </summary>
    private bool _disposed;

    /// <inheritdoc cref="GlobalWindowManager" />
    /// <param name="appLifetime">The application lifetime.</param>
    /// <param name="loggerFactory">A factory for creating loggers.</param>
    public GlobalWindowManager(AppLifetime appLifetime, ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<GlobalWindowManager>();
        _appLifetime = appLifetime;

        var splashScreen = new SplashScreen();
        _appLifetime.MainWindow = splashScreen;
        splashScreen.Show();

        _processProxy = new ProcessProxy(loggerFactory);
        _pythonProxy = new PythonProxy(_processProxy, loggerFactory);
        _appLifetime.Exit += (_, _) => OnExit();

        ImmutableList<Camera> cameras;
        try
        {
            cameras = _pythonProxy.GetAvailableCameras();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting available cameras");
            OnNavigatorShutdown(ExitCode.PythonError);
            return;
        }

        switch (cameras.Count)
        {
            case 0:
                _logger.LogError("No cameras detected");
                OnNavigatorShutdown(ExitCode.NoCamerasDetected);
                return;
            case > 1:
                _logger.LogTrace("Multiple cameras detected");
                Navigate(new CameraChooserDialog
                {
                    DataContext = new CameraChooserViewModel(cameras, Initialize)
                });
                break;
            default:
                _logger.LogTrace("Single camera detected");
                Initialize(cameras.First());
                break;
        }
    }

    /// <summary>
    /// Initializes the application.
    /// </summary>
    private void Initialize(Camera camera)
    {
        _logger.LogInformation("Selected camera: {Camera}", camera.DisplayName);

        _pythonHandler = new PythonHandler(camera.Index, _pythonProxy, _loggerFactory);
        _navigator = new Navigator(
            _appLifetime,
            _pythonHandler,
            (navigator, pythonHandler) => new ViewModelProvider(
                navigator,
                pythonHandler,
                _processProxy,
                handler => new HotspotHandler(handler, _loggerFactory),
                config => new ContentProvider(config),
                () => new LayoutProvider(_loggerFactory),
                _loggerFactory
            ),
            () => new FileHandler(_loggerFactory),
            OnNavigatorShutdown,
            _loggerFactory
        );

        _logger.LogInformation("Application initialized successfully");
    }

    /// <summary>
    /// A callback for when the navigator shuts down (usually due to an error).
    /// </summary>
    /// <param name="exitCode">The exit code.</param>
    private void OnNavigatorShutdown(ExitCode exitCode)
    {
        switch (exitCode)
        {
            case ExitCode.Success:
            {
                _logger.LogInformation("Application exited successfully");
                OnExit();
                return;
            }
            case ExitCode.ConfigNotFound:
            {
                _logger.LogTrace("No configuration file found, showing dialog");

                var dialog = new ConfirmationDialog(
                    "Configuration Not Found",
                    WarningIconPath,
                    "No configuration file was found. Would you like to open the editor to create one?",
                    "Open Editor",
                    cancelButtonText: "Close"
                );
                OpenStandaloneDialog(dialog, result =>
                {
                    if (result == Result.Confirmed)
                    {
                        _logger.LogInformation("No configuration file found, opening editor");
                        _navigator?.OpenEditor();
                    }
                    else
                    {
                        _logger.LogTrace("Dialog dismissed, exiting application");
                        OnExit();
                    }
                });
                return;
            }
            case ExitCode.NoCamerasDetected:
            case ExitCode.ConfigLoadError:
            case ExitCode.PythonError:
            default:
            {
                var message = exitCode switch
                {
                    ExitCode.NoCamerasDetected => "No compatible cameras detected",
                    ExitCode.ConfigLoadError => "Error loading configuration",
                    ExitCode.PythonError => "Error running computer vision",
                    _ => "An unknown error occurred"
                };

                var dialog = new InfoDialog(
                    "Error",
                    WarningIconPath,
                    $"{message}. See the logs for more information."
                );
                OpenStandaloneDialog(dialog, _ =>
                {
                    _logger.LogTrace("Error dialog dismissed, exiting application");
                    OnExit();
                });
                return;
            }
        }
    }

    /// <summary>
    /// Hides all windows, shows a closing dialog and <see cref="Dispose">disposes</see> of the application.
    /// </summary>
    private void OnExit()
    {
        lock (this)
        {
            if (_disposed) return;
        }

        _logger.LogTrace("Hiding windows and showing closing dialog");
        var windows = _appLifetime.Windows.ToImmutableList();
        foreach (var window in windows)
        {
            window.ShowInTaskbar = false;
            window.Hide();
        }

        new AppClosingDialog(() =>
        {
            _logger.LogTrace("Closing windows and disposing application");
            foreach (var window in windows)
                window.CloseAndDispose();

            Dispose();

            _logger.LogTrace("Application disposed, shutting down");
        }).Show();
    }

    /// <summary>
    /// Disposes of <see cref="_navigator" />.
    /// </summary>
    public void Dispose()
    {
        lock (this)
        {
            _disposed = true;
            _logger.LogTrace("Performing cleanup");

            _navigator?.Dispose();
            _navigator = null;
            _pythonHandler?.Dispose();
            _pythonHandler = null;
            _pythonProxy.Dispose();

            _logger.LogTrace("Cleanup complete");
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Navigates to a new window and <see cref="WindowExtensions.CloseAndDispose">closes</see> the old one.
    /// </summary>
    /// <param name="newWindow">The new window to navigate to.</param>
    private void Navigate(Window newWindow)
    {
        var oldWindow = _appLifetime.MainWindow;
        _appLifetime.MainWindow = newWindow;
        oldWindow?.Hide();
        newWindow.Show();
        oldWindow?.CloseAndDispose();
    }

    /// <summary>
    /// <see cref="Navigate">Navigates</see> to the specified <see cref="ResultDialog.ShowStandaloneDialog">standalone dialog</see>.
    /// </summary>
    /// <param name="dialog">The dialog to show.</param>
    /// <param name="onDialogClose">A callback for when the dialog is closed.</param>
    private void OpenStandaloneDialog(ResultDialog dialog, Action<Result> onDialogClose)
    {
        var oldWindow = _appLifetime.MainWindow;
        _appLifetime.MainWindow = dialog;
        oldWindow?.Hide();
        dialog.ShowStandaloneDialog(onDialogClose);
        oldWindow?.CloseAndDispose();
    }
}

/// <summary>
/// Possible handled exit codes.
/// </summary>
public enum ExitCode
{
    Success = 0,
    NoCamerasDetected = 100,
    ConfigLoadError = 101,
    ConfigNotFound = 102,
    PythonError = 200,
}

/// <summary>
/// Extension methods for <see cref="ExitCode"/>.
/// </summary>
public static class ExitCodeExtensions
{
    /// <summary>
    /// <inheritdoc cref="IControlledApplicationLifetime.Shutdown(int)" />
    /// </summary>
    /// <param name="appLifetime">The application lifetime.</param>
    /// <param name="exitCode">The exit code. Default is <see cref="ExitCode.Success"/>.</param>
    public static void Shutdown(this AppLifetime appLifetime, ExitCode exitCode = ExitCode.Success) =>
        appLifetime.Shutdown((int)exitCode);

    /// <summary>
    /// Checks if the integer exit code is the expected exit code.
    /// </summary>
    public static bool IsExitCode(this int code, ExitCode expected) => code == (int)expected;
}
