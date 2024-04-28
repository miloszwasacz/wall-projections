#if !DEBUGSKIPPYTHON
using System;
using System.Collections.Immutable;
using System.IO;
using Microsoft.Extensions.Logging;
using Avalonia;
using WallProjections.Helper.Interfaces;
using WallProjections.Models.Interfaces;
using Python.Runtime;
using WallProjections.Models;

#else
using System.Diagnostics.CodeAnalysis;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Avalonia;
using WallProjections.Helper.Interfaces;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
#endif

namespace WallProjections.Helper;

#if !DEBUGSKIPPYTHON
/// <inheritdoc />
public sealed class PythonProxy : IPythonProxy
{
    private const string PythonDllExceptionMessage = "Could not load Python environment.";

    /// <summary>
    /// Folder to store virtual environment.
    /// </summary>
    private const string VirtualEnvFolder = "VirtualEnv";

    /// <summary>
    /// Folder where the virtual environment is stored (if it exists).
    /// </summary>
    private static readonly string VirtualEnvPath = Path.Combine(IFileHandler.AppDataFolderPath, VirtualEnvFolder);

    /// <summary>
    /// Path to the Python executable in the virtual environment.
    /// </summary>
    /// <seealso cref="VirtualEnvPath"/>
    private static readonly string VirtualEnvExecutablePath = Path.Combine(VirtualEnvPath,
        OperatingSystem.IsWindows()
            ? @"Scripts\python"
            : "bin/python"
    );

    /// <summary>
    /// Folder where the embedded Python environment is stored (if it exists).
    /// </summary>
    private static readonly string EmbeddedEnvFolder = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "python"
    );

    /// <summary>
    /// Path to embedded Python environment executable.
    /// </summary>
    private static readonly string EmbeddedEnvExecutablePath = EmbeddedEnvFolder + (
        OperatingSystem.IsWindows()
            ? @"\python.exe"
            : "/python"
    );

    /// <summary>
    /// A handle to Python threads
    /// </summary>
    private readonly IntPtr _threadsPtr;

    /// <summary>
    /// A logger for this class
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// The currently running Python module
    /// </summary>
    private readonly AtomicPythonModule _currentModule = new();

    /// <summary>
    /// Initializes the Python runtime
    /// </summary>
    /// <exception cref="DllNotFoundException">If Python could not be initialized</exception>
    /// <remarks>Reference for VirtualEnv code: https://gist.github.com/AMArostegui/9b2ecf9d87042f2c119e417b4e38524b</remarks>
    public PythonProxy(IProcessProxy processProxy, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<PythonProxy>();
        _logger.LogInformation("Initializing Python.");

        string? pythonExecutablePath = null;

        if (File.Exists(EmbeddedEnvExecutablePath))
        {
            _logger.LogInformation("Embedded Python environment detected.");
            _logger.LogTrace("Python executable path: {Path}", EmbeddedEnvExecutablePath);
            pythonExecutablePath = EmbeddedEnvExecutablePath;
        }
        else if (File.Exists(VirtualEnvExecutablePath))
        {
            _logger.LogInformation("Virtual Python environment detected.");
            _logger.LogTrace("Python executable path: {Path}", VirtualEnvExecutablePath);
            pythonExecutablePath = VirtualEnvExecutablePath;
        }

        // Only use embedded environment if executable exists.
        if (pythonExecutablePath is not null)
        {
            // TODO: Show error message to user before shutting down program if environment cannot be loaded.
            try
            {
                var (pythonDll, pythonPath) = processProxy.LoadPythonEnv(pythonExecutablePath);
                Runtime.PythonDLL = pythonDll;
                PythonEngine.PythonPath = pythonPath;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not load Python non-global environment.");
                throw new DllNotFoundException(PythonDllExceptionMessage, e);
            }
        }
        else
        {
            _logger.LogInformation("No non-global environment detected. Loading Python from system.");
            Runtime.PythonDLL = Environment.GetEnvironmentVariable("PYTHON_DLL");
            if (Runtime.PythonDLL is null)
            {
                _logger.LogError("PythonDLL environment variable not set.");
                throw new DllNotFoundException(PythonDllExceptionMessage);
            }
        }

        PythonEngine.Initialize();
        _threadsPtr = PythonEngine.BeginAllowThreads();
        _logger.LogInformation("Python initialized.");
    }

    /// <inheritdoc />
    public void StartHotspotDetection(IPythonHandler eventListener, IConfig config) =>
        RunPythonAction(PythonModule.HotspotDetection, module =>
        {
            _logger.LogInformation("Starting hotspot detection.");
            module.StartDetection(eventListener, config);
        });

    /// <inheritdoc />
    public void StopCurrentAction()
    {
        var currentModule = _currentModule.Take();
        if (currentModule is not PythonModule.HotspotDetectionModule module) return;

        using (Py.GIL())
        {
            _logger.LogInformation("Stopping hotspot detection.");
            module.StopDetection();
        }
    }

    /// <inheritdoc />
    public double[,]? CalibrateCamera(int cameraIndex, ImmutableDictionary<int, Point> arucoPositions) =>
        RunPythonAction(PythonModule.Calibration, module =>
        {
            _logger.LogInformation("Calibrating camera.");
            return module.CalibrateCamera(cameraIndex, arucoPositions);
        });

    /// <inheritdoc />
    public ImmutableList<Camera> GetAvailableCameras() =>
        RunPythonAction(PythonModule.CameraIdentification, module =>
        {
            _logger.LogInformation("Identifying available cameras.");
            return module.GetAvailableCameras(_logger);
        });

    /// <summary>
    /// Runs the given action after acquiring the Python GIL importing the given module
    /// </summary>
    /// <param name="moduleFactory">A factory for creating the Python module</param>
    /// <param name="action">The action to run using the Python module</param>
    /// <typeparam name="T">The type of the Python module</typeparam>
    /// <remarks><see cref="AtomicPythonModule.Set">Sets</see> <see cref="_currentModule" /> to the imported module</remarks>
    private void RunPythonAction<T>(Func<T> moduleFactory, Action<T> action) where T : PythonModule
    {
        using (Py.GIL())
        {
            var module = moduleFactory();
            _currentModule.Set(module);
            action(module);
        }
    }

    /// <summary>
    /// Runs the given action after acquiring the Python GIL importing the given module and returns the result
    /// </summary>
    /// <param name="moduleFactory">A factory for creating the Python module</param>
    /// <param name="action">The action to run using the Python module</param>
    /// <typeparam name="T">The type of the Python module</typeparam>
    /// <typeparam name="TR">The return type of the action</typeparam>
    /// <remarks><see cref="AtomicPythonModule.Set">Sets</see> <see cref="_currentModule" /> to the imported module</remarks>
    private TR RunPythonAction<T, TR>(Func<T> moduleFactory, Func<T, TR> action) where T : PythonModule
    {
        using (Py.GIL())
        {
            var module = moduleFactory();
            _currentModule.Set(module);
            return action(module);
        }
    }

    public void Dispose()
    {
        PythonEngine.EndAllowThreads(_threadsPtr);
        PythonEngine.Shutdown();
    }

    /// <summary>
    /// A thread-safe wrapper around a Python module
    /// </summary>
    private class AtomicPythonModule
    {
        /// <summary>
        /// The internal module reference
        /// </summary>
        /// <remarks>
        /// Remember to lock on <i>this</i> when accessing this field
        /// </remarks>
        private PythonModule? _module;

        /// <summary>
        /// Sets the module, replacing the previous one (if any)
        /// </summary>
        public void Set(PythonModule module)
        {
            lock (this)
            {
                _module = module;
            }
        }

        /// <summary>
        /// Takes the module, i.e. returns it and sets the internal reference to <i>null</i>
        /// </summary>
        /// <returns>The previously held module, or <i>null</i> if there was none</returns>
        public PythonModule? Take()
        {
            lock (this)
            {
                var module = _module;
                _module = null;
                return module;
            }
        }
    }
}
#else
/// <summary>
/// A mock of <see cref="IPythonProxy" /> in an environment without Python
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Mock Python proxy for manual testing")]
public sealed class PythonProxy : IPythonProxy
{
    /// <summary>
    /// A logger for this class
    /// </summary>
    private readonly ILogger _logger;

    // ReSharper disable UnusedParameter.Local
    /// <summary>
    /// Constructor to keep definitions consistent with the Python version.
    /// </summary>
    public PythonProxy(IProcessProxy processProxy, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<PythonProxy>();
    }
    // ReSharper restore UnusedParameter.Local

    /// <summary>
    /// Prints a message to the console
    /// </summary>
    public void StartHotspotDetection(IPythonHandler eventListener, IConfig config)
    {
        _logger.LogInformation("Starting hotspot detection");
    }

    /// <summary>
    /// Prints a message to the console
    /// </summary>
    public void StopCurrentAction()
    {
        _logger.LogInformation("Stopping currently running action");
    }

    /// <summary>
    /// Prints a message to the console and returns an identity matrix
    /// </summary>
    public double[,] CalibrateCamera(int cameraIndex, ImmutableDictionary<int, Point> arucoPositions)
    {
        _logger.LogInformation("Calibrating camera");
        return new double[,]
        {
            { 1, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 1 }
        };
    }

    public ImmutableList<Camera> GetAvailableCameras()
    {
        _logger.LogInformation("Identifying available cameras");
        Task.Delay(2000).Wait();
        return ImmutableList.Create(
            new Camera(0, "Camera 0"),
            new Camera(700, "Camera 1"),
            new Camera(702, "Camera 2")
        );
    }

    /// <summary>
    /// Prints a message to the console
    /// </summary>
    public void Dispose()
    {
        _logger.LogInformation("Disposing PythonProxy");
    }
}
#endif
