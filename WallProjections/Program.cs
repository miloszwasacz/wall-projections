using System;
using System.Runtime.CompilerServices;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.ReactiveUI;
using WallProjections.ViewModels;
using Python.Runtime;
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
        var pythonThread = InitializePython();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        pythonThread.Cancel();
        pythonThread.Dispose();
        ViewModelProvider.Instance.Dispose();
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

    /// <summary>
    /// Initializes the Python engine on a separate thread
    /// </summary>
    /// <returns>A handle to cancel the task after the app is closed</returns>
    private static CancellationTokenSource InitializePython()
    {
        Runtime.PythonDLL = Environment.GetEnvironmentVariable("PYTHON_DLL");

        // Run Python in a separate thread
        var cts = new CancellationTokenSource();
        Task.Run(() =>
        {
            PythonEngine.Initialize();
            Py.GIL();
            using var scope = Py.CreateScope();
            string code = File.ReadAllText("Scripts/main.py");
            var scriptCompiled = PythonEngine.Compile(code);
            scope.Execute(scriptCompiled);
            //TODO Change to a real method
            scope.InvokeMethod("detect_buttons", PythonEventHandler.Instance.ToPython());
        }, cts.Token);
        return cts;
    }
}
