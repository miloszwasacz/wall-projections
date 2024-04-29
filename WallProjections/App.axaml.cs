using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging;
using WallProjections.Helper.Interfaces;

namespace WallProjections;

public class App : Application
{
    /// <summary>
    /// A factory for creating loggers
    /// </summary>
    private readonly ILoggerFactory _loggerFactory;

    // ReSharper disable once NotAccessedField.Local
    /// <summary>
    /// The application-wide navigator
    /// </summary>
    private GlobalWindowManager? _windowManager;

    public App(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        InitializeNavigator(ApplicationLifetime);
        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Initializes the application-wide window manager.
    /// </summary>
    /// <param name="lifetime">The application lifetime.</param>
    [MethodImpl(MethodImplOptions.NoOptimization)] // Prevents _windowManager from being optimized away
    [ExcludeFromCodeCoverage(Justification = "Headless lifetime is not a IClassicDesktopStyleApplicationLifetime")]
    private void InitializeNavigator(IApplicationLifetime? lifetime)
    {
        if (lifetime is IClassicDesktopStyleApplicationLifetime desktop)
            _windowManager = new GlobalWindowManager(desktop, _loggerFactory);
    }

#if DEBUGSKIPPYTHON
    /// <summary>
    /// A property to access the Python handler for mocking input
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Used only in manual testing")]
    public IPythonHandler PythonHandler => _windowManager!.PythonHandler;
#endif

    /// <summary>
    /// This constructor should not be called. It is only here for the visual designer.
    /// Use <see cref="App(ILoggerFactory)" /> instead.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("This constructor should not be called. See the documentation for more information.", true)]
    public App()
    {
        _loggerFactory = null!;
    }
}
