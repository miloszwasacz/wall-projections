using System;
using System.Threading;
using System.Threading.Tasks;
using Python.Runtime;
using WallProjections.Helper.Interfaces;
#if !DEBUGSKIPPYTHON
using System.Diagnostics;
#endif

namespace WallProjections.Helper;

/// <summary>
/// The event handler singleton for Python interop
/// </summary>
public sealed class PythonHandler : IPythonHandler
{
    //TODO Use an enum for the module names
    private const string HotspotDetectionModule = "hotspot_detection";
    private const string CalibrationModule = "calibration";

#if !DEBUGSKIPPYTHON
    /// <summary>
    /// A handle to Python threads
    /// </summary>
    private readonly IntPtr _threadsPtr;
#endif

    /// <summary>
    /// The backing field for <see cref="Instance" />
    /// </summary>
    private static PythonHandler? _instance;

    /// <summary>
    /// The global instance of the event handler
    /// </summary>
    /// <remarks>If possible, don't use this global instance - use Dependency Injection instead</remarks>
    /// <exception cref="TypeInitializationException">
    /// If the global instance has not been <see cref="Initialize">initialized</see>
    /// </exception>
    public static PythonHandler Instance =>
        _instance ?? throw new TypeInitializationException(nameof(PythonHandler), null);

    /// <summary>
    /// Initializes the global instance of the Python event handler
    /// </summary>
    /// <exception cref="InvalidOperationException">If the global instance has already been initialized</exception>
    public static IPythonHandler Initialize()
    {
        if (_instance != null)
            throw new InvalidOperationException("PythonEventHandler has already been initialized");

        _instance = new PythonHandler();
        return _instance;
    }

    /// <summary>
    /// Creates a new instance of <see cref="PythonHandler" /> and initializes the Python runtime
    /// </summary>
    private PythonHandler()
    {
#if !DEBUGSKIPPYTHON
        Runtime.PythonDLL = Environment.GetEnvironmentVariable("PYTHON_DLL");
        Debug.Write("Initializing Python...    ");
        PythonEngine.Initialize();
        _threadsPtr = PythonEngine.BeginAllowThreads();
        Debug.WriteLine("Done");
#endif
    }

    /// <summary>
    /// The currently running Python task
    /// </summary>
    private CancellationTokenSource? _currentTask;

    /// <inheritdoc />
    public Task RunHotspotDetection() =>
        RunNewPythonAction(HotspotDetectionModule, module => { module.run(this.ToPython()); });

    /// <inheritdoc />
    public Task RunCalibration() =>
        RunNewPythonAction(CalibrationModule, module => { module.test(); });

    /// <summary>
    /// Cancels the current task and calls <see cref="RunPythonAction" /> on a separate thread.
    /// </summary>
    /// <param name="moduleName">The module name passed to <see cref="RunPythonAction" /></param>
    /// <param name="action">The action passed to <see cref="RunPythonAction" /></param>
    private async Task RunNewPythonAction(string moduleName, Action<dynamic> action)
    {
        CancelCurrentTask();
        _currentTask = new CancellationTokenSource();
        await Task.Run(() => RunPythonAction(moduleName, action), _currentTask.Token);
    }

    // ReSharper disable UnusedParameter.Local
    /// <summary>
    /// Runs the given action after acquiring the Python GIL importing the given module
    /// </summary>
    /// <param name="moduleName">The name of the Python module to import</param>
    /// <param name="action">The action to run using the Python module</param>
    private static void RunPythonAction(string moduleName, Action<dynamic> action)
    {
        try
        {
#if !DEBUGSKIPPYTHON
            using (Py.GIL())
            {
                //TODO Import a module once and reuse it
                dynamic module = Py.Import($"Scripts.{moduleName}");
                action(module);
            }
#endif
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            throw;
        }
    }
    // ReSharper restore UnusedParameter.Local

    /// <inheritdoc />
    public void CancelCurrentTask()
    {
        RunPythonAction(HotspotDetectionModule, module => module.stop_hotspot_detection());
        _currentTask?.Cancel();
        _currentTask?.Dispose();
        _currentTask = null;
    }

    /// <inheritdoc />
    public event EventHandler<IPythonHandler.HotspotSelectedArgs>? HotspotSelected;

    /// <inheritdoc />
    public void OnPressDetected(int id)
    {
        HotspotSelected?.Invoke(this, new IPythonHandler.HotspotSelectedArgs(id));
    }

    public void Dispose()
    {
        CancelCurrentTask();
#if !DEBUGSKIPPYTHON
        PythonEngine.EndAllowThreads(_threadsPtr);
        PythonEngine.Shutdown();
#endif
    }
}
