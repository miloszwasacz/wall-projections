using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.ReactiveUI;
using WallProjections.Helper;

[assembly: InternalsVisibleTo("WallProjections.Test")]

namespace WallProjections;

// ReSharper disable once ClassNeverInstantiated.Global
internal class Program
{
    /// <summary>
    /// Initialization code. Don't use any Avalonia, third-party APIs or any
    /// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    /// yet and stuff might break.
    /// </summary>
    /// <param name="args">Application arguments</param>
    [STAThread]
    public static void Main(string[] args)
    {
        var pythonHandler = PythonHandler.Initialize();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        pythonHandler.Dispose();
    }

    /// <summary>
    /// Avalonia configuration, don't remove; also used by visual designer.
    /// </summary>
    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
    }
}
