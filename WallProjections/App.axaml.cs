using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging;
using WallProjections.Helper;
using WallProjections.Helper.Interfaces;
using WallProjections.Models;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Display.Layouts;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections;

public class App : Application
{
    /// <summary>
    /// The application-wide Python handler for interop
    /// </summary>
    private readonly IPythonHandler _pythonHandler;

    /// <summary>
    /// A factory for creating loggers
    /// </summary>
    private readonly ILoggerFactory _loggerFactory;

    // ReSharper disable once NotAccessedField.Local
    /// <summary>
    /// The application-wide navigator
    /// </summary>
    private INavigator? _navigator;

    public App(IPythonHandler pythonHandler, ILoggerFactory loggerFactory)
    {
        _pythonHandler = pythonHandler;
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
    /// Initializes the application-wide navigator.
    /// </summary>
    /// <param name="lifetime">The application lifetime.</param>
    [MethodImpl(MethodImplOptions.NoOptimization)] // Prevents _navigator from being optimized away
    [ExcludeFromCodeCoverage(Justification = "Headless lifetime is not a IClassicDesktopStyleApplicationLifetime")]
    private void InitializeNavigator(IApplicationLifetime? lifetime)
    {
        if (lifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _navigator = new Navigator(
                desktop,
                _pythonHandler,
                (nav, pythonHandler) => new ViewModelProvider(
                    nav,
                    pythonHandler,
                    new ProcessProxy(_loggerFactory),
                    pyHandler => new HotspotHandler(pyHandler, _loggerFactory),
                    config => new ContentProvider(config),
                    () => new LayoutProvider(_loggerFactory),
                    _loggerFactory
                ),
                () => new FileHandler(),
                _loggerFactory
            );
        }
    }

#if DEBUGSKIPPYTHON
    // ReSharper disable once ConvertToAutoPropertyWhenPossible
    /// <summary>
    /// A property to access the Python handler for mocking input
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Used only in manual testing")]
    public IPythonHandler PythonHandler => _pythonHandler;
#endif

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// This constructor should not be called. It is only here to suppress an Avalonia warning.
    /// Use <see cref="App(IPythonHandler, ILoggerFactory)" /> instead.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
    [ExcludeFromCodeCoverage]
    [Obsolete("This constructor should not be called. See the documentation for more information.", true)]
    public App() => throw new InvalidOperationException(
        "This constructor should not be called. See the documentation for more information."
    );
}
