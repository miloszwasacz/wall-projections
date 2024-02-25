using System;
using System.Threading;
using System.Threading.Tasks;
using WallProjections.Helper.Interfaces;

namespace WallProjections.Helper;

/// <summary>
/// The event handler singleton for Python interop
/// </summary>
public sealed class PythonHandler : IPythonHandler
{
    /// <summary>
    /// The backing field for <see cref="Instance" />
    /// </summary>
    private static PythonHandler? _instance;

    /// <summary>
    /// The global instance of the event handler
    /// </summary>
    /// <remarks>If possible, <b>don't use this global instance</b> - use Dependency Injection instead</remarks>
    /// <exception cref="TypeInitializationException">
    /// If the global instance has not been <see cref="Initialize">initialized</see>
    /// </exception>
    public static PythonHandler Instance =>
        _instance ?? throw new TypeInitializationException(nameof(PythonHandler), null);

    /// <summary>
    /// Initializes the global instance of the Python event handler
    /// </summary>
    /// <param name="pythonProxy">The proxy used for executing Python scripts</param>
    /// <exception cref="InvalidOperationException">If the global instance has already been initialized</exception>
    public static IPythonHandler Initialize(IPythonProxy pythonProxy)
    {
        if (_instance != null)
            throw new InvalidOperationException("PythonEventHandler has already been initialized");

        _instance = new PythonHandler(pythonProxy);
        return _instance;
    }

    /// <summary>
    /// Creates a new instance of <see cref="PythonHandler" /> which uses the given <see cref="IPythonProxy" />
    /// </summary>
    private PythonHandler(IPythonProxy pythonProxy)
    {
        _pythonProxy = pythonProxy;
    }

    //TODO Maybe exclude this when marshalling to a Python object?
    /// <summary>
    /// A proxy for setting up Python runtime and executing Python scripts
    /// </summary>
    private readonly IPythonProxy _pythonProxy;

    /// <summary>
    /// A mutex to ensure sequential access to <see cref="_currentTask" />
    /// </summary>
    private readonly Mutex _taskMutex = new();

    /// <summary>
    /// The currently running Python task
    /// </summary>
    private CancellationTokenSource? _currentTask;

    /// <inheritdoc />
    public Task RunHotspotDetection() => RunNewPythonAction(python => python.StartHotspotDetection(this));

    /// <inheritdoc />
    public Task RunCalibration() => RunNewPythonAction(python => python.CalibrateCamera());

    /// <summary>
    /// Cancels the current task and calls <see cref="RunPythonAction" /> on a separate thread.
    /// </summary>
    /// <param name="action">The action passed to <see cref="RunPythonAction" /></param>
    private async Task RunNewPythonAction(Action<IPythonProxy> action)
    {
        CancelCurrentTask();
        _taskMutex.WaitOne();
        var task = _currentTask = new CancellationTokenSource();
        _taskMutex.ReleaseMutex();
        await Task.Run(() =>
        {
            try
            {
                RunPythonAction(action);
            }
            catch (Exception)
            {
                if (task.IsCancellationRequested)
                {
                    //TODO Log to file
                    Console.Out.WriteLine("Task was cancelled");
                }
                else throw;
            }
        }, task.Token);
    }

    /// <summary>
    /// Runs the given action using a <see cref="IPythonProxy" />.
    /// </summary>
    /// <param name="action">
    /// The action to execute (it should involve calling a method on the input <see cref="IPythonProxy" />)
    /// </param>
    private void RunPythonAction(Action<IPythonProxy> action)
    {
        try
        {
            action(_pythonProxy);
        }
        catch (Exception e)
        {
            //TODO Log to file
            Console.Error.WriteLine(e);
            throw;
        }
    }

    /// <inheritdoc />
    public void CancelCurrentTask()
    {
        _taskMutex.WaitOne();
        if (_currentTask is null)
        {
            _taskMutex.ReleaseMutex();
            return;
        }

        RunPythonAction(python => python.StopCurrentAction());
        _currentTask?.Cancel();
        _currentTask?.Dispose();
        _currentTask = null;

        _taskMutex.ReleaseMutex();
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
    }
}
