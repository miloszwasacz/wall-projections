using System;
using System.Linq;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.Logging;
using WallProjections.Helper;
using WallProjections.Helper.Interfaces;
using WallProjections.Models;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Display.Layouts;
using WallProjections.ViewModels.Interfaces;
using AppLifetime = Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;

namespace WallProjections;

/// <summary>
/// Manages the global application state.
/// </summary>
public class GlobalWindowManager
{
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

    /// <inheritdoc cref="GlobalWindowManager" />
    /// <param name="appLifetime">The application lifetime.</param>
    /// <param name="loggerFactory">A factory for creating loggers.</param>
    public GlobalWindowManager(AppLifetime appLifetime, ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<GlobalWindowManager>();
        _appLifetime = appLifetime;
        // Show splash screen
        _processProxy = new ProcessProxy(loggerFactory);
        _pythonProxy = new PythonProxy(_processProxy, loggerFactory);
        _appLifetime.Exit += OnExit;
        Initialize();
    }

    /// <summary>
    /// Initializes the application.
    /// </summary>
    private void Initialize()
    {
        var cameras = _pythonProxy.GetAvailableCameras();
        int cameraIndex;
        switch (cameras.Count)
        {
            case 0:
                _logger.LogError("No cameras detected");
                _appLifetime.Shutdown(ExitCode.NoCamerasDetected);
                return;
            case > 1:
                _logger.LogTrace("Multiple cameras detected");
                // Show dialog to select camera if multiple cameras are detected
                //TODO Allow user to select camera
                cameraIndex = cameras.First().Key;
                break;
            default:
                _logger.LogTrace("Single camera detected");
                cameraIndex = cameras.First().Key;
                break;
        }

        _pythonHandler = new PythonHandler(cameraIndex, _pythonProxy, _loggerFactory);
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
            () => new FileHandler(),
            _loggerFactory
        );
    }

    /// <summary>
    /// Handles the application exit event.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments, containing the exit code.</param>
    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        if (!e.ApplicationExitCode.IsExitCode(ExitCode.Success))
        {
            //TODO Show a dialog on error
        }

        _navigator?.Dispose();
        _pythonHandler?.Dispose();
        _pythonProxy.Dispose();
    }
}

/// <summary>
/// Possible handled exit codes.
/// </summary>
public enum ExitCode
{
    Success = 0,
    NoCamerasDetected = 100
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
