using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using WallProjections.Helper;
using WallProjections.Models;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Display.Layouts;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections;

public class App : Application
{
    // ReSharper disable once NotAccessedField.Local
    private INavigator? _navigator;

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
    [ExcludeFromCodeCoverage(Justification = "Headless lifetime is not a IClassicDesktopStyleApplicationLifetime")]
    private void InitializeNavigator(IApplicationLifetime? lifetime)
    {
        if (lifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _navigator = new Navigator(
                desktop,
                PythonHandler.Instance,
                (nav, pythonHandler) => new ViewModelProvider(
                    nav,
                    pythonHandler,
                    new ProcessProxy(),
                    config => new ContentProvider(config),
                    () => new LayoutProvider()
                ),
                () => new FileHandler()
            );
        }
    }
}
