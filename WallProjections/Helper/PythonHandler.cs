using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Microsoft.Extensions.Logging;
using WallProjections.Helper.Interfaces;
using WallProjections.Models.Interfaces;

namespace WallProjections.Helper;

/// <summary>
/// The event handler for Python interop
/// </summary>
/// <remarks>Should only be instantiated once in <see cref="Program" /></remarks>
public sealed class PythonHandler : IPythonHandler
{
    /// <summary>
    /// Creates a new instance of <see cref="PythonHandler" /> which uses the given <see cref="IPythonProxy" />
    /// </summary>
    /// <param name="cameraIndex">The index of the camera that will be passed to OpenCV</param>
    /// <param name="pythonProxy">A proxy for setting up Python runtime and executing Python scripts</param>
    /// <param name="loggerFactory">A factory for creating loggers</param>
    public PythonHandler(int cameraIndex, IPythonProxy pythonProxy, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<PythonHandler>();
        _pythonProxy = pythonProxy;
        CameraIndex = cameraIndex;
    }

    /// <summary>
    /// A logger for this class
    /// </summary>
    private readonly ILogger _logger;

    //TODO Maybe exclude this when marshalling to a Python object?
    /// <summary>
    /// A proxy for setting up Python runtime and executing Python scripts
    /// </summary>
    private readonly IPythonProxy _pythonProxy;

    /// <summary>
    /// The currently running Python task
    /// </summary>
    private CancellationTokenSource? _currentTask;

    /// <inheritdoc />
    public int CameraIndex { get; }

    /// <inheritdoc />
    public Task RunHotspotDetection(IConfig config) =>
        RunNewPythonAction(python => python.StartHotspotDetection(this, config), "Hotspot Detection");

    /// <inheritdoc />
    public Task<double[,]?> RunCalibration(ImmutableDictionary<int, Point> arucoPositions) =>
        RunNewPythonAction(python => python.CalibrateCamera(CameraIndex, arucoPositions), "Calibration");

    /// <inheritdoc />
    public void CancelCurrentTask()
    {
        lock (this)
        {
            if (_currentTask is null)
                return;

            try
            {
                RunPythonAction(python => python.StopCurrentAction(), "Stop Current Action");
            }
            finally
            {
                _currentTask?.Cancel();
                _currentTask?.Dispose();
                _currentTask = null;
            }
        }
    }

    #region Running Python Actions

    /// <summary>
    /// Cancels the current task and calls <see cref="RunPythonAction">RunPythonAction</see> on a separate thread.
    /// </summary>
    /// <param name="action">The action passed to <see cref="RunPythonAction" /></param>
    /// <param name="actionName">The name of the action (for logging purposes)</param>
    private Task RunNewPythonAction(Action<IPythonProxy> action, string actionName)
    {
        CancelCurrentTask();
        CancellationTokenSource task;
        lock (this)
        {
            task = _currentTask = new CancellationTokenSource();
        }

        return Task.Run(() =>
        {
            try
            {
                RunPythonAction(action, actionName);
            }
            catch (Exception)
            {
                if (task.IsCancellationRequested)
                    throw new TaskCanceledException();

                throw;
            }
        }, task.Token);
    }

    /// <summary>
    /// Cancels the current task and calls <see cref="RunPythonAction{TR}">RunPythonAction</see> on a separate thread.
    /// </summary>
    /// <param name="action">The action passed to <see cref="RunPythonAction{TR}" /></param>
    /// <param name="actionName">The name of the action (for logging purposes)</param>
    /// <typeparam name="TR">The return type of the action</typeparam>
    /// <returns>The result of the action</returns>
    private Task<TR?> RunNewPythonAction<TR>(Func<IPythonProxy, TR?> action, string actionName)
    {
        CancelCurrentTask();
        CancellationTokenSource task;
        lock (this)
        {
            task = _currentTask = new CancellationTokenSource();
        }

        return Task.Run(() =>
        {
            try
            {
                return RunPythonAction(action, actionName);
            }
            catch (Exception)
            {
                if (task.IsCancellationRequested)
                    throw new TaskCanceledException();

                throw;
            }
        }, task.Token);
    }

    /// <summary>
    /// Runs the given action using a <see cref="IPythonProxy" />.
    /// </summary>
    /// <param name="action">
    /// The action to execute (it should involve calling a method on the input <see cref="IPythonProxy" />)
    /// </param>
    /// <param name="actionName">The name of the action (for logging purposes)</param>
    /// <exception cref="Exception">If an error occurs while running the action</exception>
    private void RunPythonAction(Action<IPythonProxy> action, string actionName)
    {
        try
        {
            action(_pythonProxy);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error running Python action ({ActionName})", actionName);
            throw;
        }
    }

    /// <summary>
    /// Runs the given action using a <see cref="IPythonProxy" /> and returns the result.
    /// </summary>
    /// <param name="action">
    /// The action to execute (it should involve calling a method on the input <see cref="IPythonProxy" />)
    /// </param>
    /// <param name="actionName">The name of the action (for logging purposes)</param>
    /// <typeparam name="TR">The return type of the action</typeparam>
    /// <returns>The result of the action</returns>
    /// <exception cref="Exception">If an error occurs while running the action</exception>
    private TR RunPythonAction<TR>(Func<IPythonProxy, TR> action, string actionName)
    {
        try
        {
            return action(_pythonProxy);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error running Python action ({ActionName})", actionName);
            throw;
        }
    }

    #endregion

    /// <inheritdoc />
    public event EventHandler<IHotspotHandler.HotspotArgs>? HotspotPressed;

    /// <inheritdoc />
    public event EventHandler<IHotspotHandler.HotspotArgs>? HotspotReleased;

    /// <inheritdoc />
    public void OnHotspotPressed(int id)
    {
        _logger.LogTrace("Hotspot {HotspotId} pressed", id);
        HotspotPressed?.Invoke(this, new IHotspotHandler.HotspotArgs(id));
    }

    /// <inheritdoc />
    public void OnHotspotUnpressed(int id)
    {
        _logger.LogTrace("Hotspot {HotspotId} released", id);
        HotspotReleased?.Invoke(this, new IHotspotHandler.HotspotArgs(id));
    }

    public void Dispose()
    {
        CancelCurrentTask();
    }
}
