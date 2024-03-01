using System;
#if !DEBUGSKIPPYTHON
using System.Threading;
using System.Diagnostics;
using Python.Runtime;
#else
using System.Diagnostics.CodeAnalysis;
#endif
using WallProjections.Helper.Interfaces;

namespace WallProjections.Helper;

#if !DEBUGSKIPPYTHON
/// <inheritdoc />
public sealed class PythonProxy : IPythonProxy
{
    /// <summary>
    /// A handle to Python threads
    /// </summary>
    private readonly IntPtr _threadsPtr;

    /// <summary>
    /// The currently running Python module for hotspot detection
    /// </summary>
    private readonly AtomicPythonModule _currentModule = new();

    /// <summary>
    /// Initializes the Python runtime
    /// </summary>
    public PythonProxy()
    {
        Runtime.PythonDLL = Environment.GetEnvironmentVariable("PYTHON_DLL");
        Debug.Write("Initializing Python...    ");
        PythonEngine.Initialize();
        _threadsPtr = PythonEngine.BeginAllowThreads();
        Debug.WriteLine("Done");
    }

    /// <inheritdoc />
    public void StartHotspotDetection(IPythonHandler eventListener)
    {
        RunPythonAction(PythonModule.HotspotDetection, module => { module.hotspot_detection(eventListener.ToPython()); });
    }

    /// <inheritdoc />
    public void StopCurrentAction()
    {
        var currentModule = _currentModule.Take();
        if (currentModule is not PythonModule.HotspotDetectionModule module) return;

        using (Py.GIL())
        {
            module.Module.stop_hotspot_detection();
        }
    }

    /// <inheritdoc />
    public void CalibrateCamera()
    {
        //TODO Change to the actual entrypoint
        RunPythonAction(PythonModule.Calibration, module => { module.calibrate(); });
    }

    /// <summary>
    /// Runs the given action after acquiring the Python GIL importing the given module
    /// </summary>
    /// <param name="moduleFactory">A factory for creating the Python module</param>
    /// <param name="action">The action to run using the Python module</param>
    /// <remarks><see cref="AtomicPythonModule.Set">Sets</see> <see cref="_currentModule" /> to the imported module</remarks>
    private void RunPythonAction(Func<PythonModule> moduleFactory, Action<dynamic> action)
    {
        using (Py.GIL())
        {
            //TODO Maybe Import a module once and reuse it?
            var module = moduleFactory();
            _currentModule.Set(module);
            action(module.Module);
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
        private readonly Mutex _mutex = new();
        private PythonModule? _module;

        /// <summary>
        /// Sets the module, replacing the previous one (if any)
        /// </summary>
        public void Set(PythonModule module)
        {
            _mutex.WaitOne();
            _module = module;
            _mutex.ReleaseMutex();
        }

        /// <summary>
        /// Takes the module, i.e. returns it and sets the internal reference to <i>null</i>
        /// </summary>
        /// <returns>The previously held module, or <i>null</i> if there was none</returns>
        public PythonModule? Take()
        {
            _mutex.WaitOne();
            var module = _module;
            _module = null;
            _mutex.ReleaseMutex();
            return module;
        }
    }

    /// <summary>
    /// Available Python modules
    /// </summary>
    private abstract class PythonModule
    {
        /// <summary>
        /// The base for importing Python modules
        /// </summary>
        /// <seealso cref="PythonModule(string)" />
        private const string ScriptPath = "Scripts";

        /// <inheritdoc cref="HotspotDetectionModule" />
        public static Func<PythonModule> HotspotDetection => () => new HotspotDetectionModule();

        /// <inheritdoc cref="CalibrationModule" />
        public static Func<PythonModule> Calibration => () => new CalibrationModule();

        /// <summary>
        /// The Python module object that this class wraps
        /// </summary>
        public dynamic Module { get; }

        /// <summary>
        /// <see cref="Py.Import">Imports</see> the Python module with the given <paramref name="name" />
        /// using the <see cref="ScriptPath" /> as the base (i.e. <i>{ScriptPath}.{name}</i>)
        /// </summary>
        /// <param name="name">The name of the Python module</param>
        private PythonModule(string name)
        {
            Module = Py.Import($"{ScriptPath}.{name}");
        }

        /// <summary>
        /// A module that provides hotspot detection functionality using Computer Vision
        /// </summary>
        public sealed class HotspotDetectionModule : PythonModule
        {
            public HotspotDetectionModule() : base("hotspot_detection")
            {
            }
        }

        /// <summary>
        /// A module that calibrates the camera to correct offset from the projector
        /// </summary>
        public sealed class CalibrationModule : PythonModule
        {
            public CalibrationModule() : base("calibration")
            {
            }
        }
    }
}
#else
/// <summary>
/// A mock of <see cref="IPythonProxy" /> in an environment without Python
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class PythonProxy : IPythonProxy
{
    /// <summary>
    /// Prints a message to the console
    /// </summary>
    public void StartHotspotDetection(IPythonHandler eventListener)
    {
        Console.WriteLine("Starting hotspot detection");
    }

    /// <summary>
    /// Prints a message to the console
    /// </summary>
    public void StopCurrentAction()
    {
        Console.WriteLine("Stopping currently running action");
    }

    /// <summary>
    /// Prints a message to the console
    /// </summary>
    public void CalibrateCamera()
    {
        Console.WriteLine("Calibrating camera");
    }

    /// <summary>
    /// Prints a message to the console
    /// </summary>
    public void Dispose()
    {
        Console.WriteLine("Disposing PythonProxy");
    }
}
#endif
