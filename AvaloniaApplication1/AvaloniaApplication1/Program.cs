using Avalonia;
using Avalonia.ReactiveUI;
using System;
using Microsoft.Extensions.Configuration;
using Python.Runtime;

namespace AvaloniaApplication1;

internal class Program
{
    private static readonly string PythonDll = "PYTHON_DLL";

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var config = GetConfig();
        var pythonPath = config?[PythonDll];
        Runtime.PythonDLL = pythonPath;
        PythonEngine.Initialize();
        Py.GIL();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseSkia()
            .UseReactiveUI();

    private static IConfigurationRoot? GetConfig() =>
        new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();
}