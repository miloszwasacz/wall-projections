using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.ReactiveUI;
using Python.Runtime;
using WallProjections.Models;
using WallProjections.ViewModels;
#if !DEBUGSKIPPYTHON
using System.Diagnostics;
using System.IO;
using WallProjections.Helper;
#endif

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
        // FileHandler.Instance.Dispose();
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
        //TODO Include this in the setup guide
        Runtime.PythonDLL = Environment.GetEnvironmentVariable("PYTHON_DLL");

        // Run Python in a separate thread
        var cts = new CancellationTokenSource();
        Task.Run(() =>
        {
#if !DEBUGSKIPPYTHON
            try
            {
                Console.WriteLine("Initializing Python...");
                PythonEngine.Initialize();
                Debug.WriteLine("- Initializing GIL");
                using (Py.GIL())
                {
                    Debug.WriteLine("- Creating scope");
                    using var scope = Py.CreateScope();
                    Debug.WriteLine("- Reading script");
                    var code = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts/main.py"));
                    Debug.WriteLine("- Compiling script");
                    var scriptCompiled = PythonEngine.Compile(code);
                    Debug.WriteLine("- Executing script");
                    scope.Execute(scriptCompiled);
                    Console.WriteLine("Initialization complete");
                    //TODO Change to a real method
                    scope.InvokeMethod("detect_buttons", PythonEventHandler.Instance.ToPython());
                    Debug.WriteLine("Python method called");
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                throw;
            }
#endif
        }, cts.Token);
        return cts;
    }
}
