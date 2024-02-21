using System;
using System.Diagnostics.CodeAnalysis;
#if !DEBUGSKIPPYTHON
using System.Diagnostics;
using Python.Runtime;
#endif
using WallProjections.Helper.Interfaces;

namespace WallProjections.Helper;

#if !DEBUGSKIPPYTHON
/// <inheritdoc />
public sealed class PythonProxy : IPythonProxy
{
    private const string ScriptPath = "Scripts";

    //TODO Use an enum for the module names
    private const string HotspotDetectionModule = "hotspot_detection";
    private const string CalibrationModule = "calibration";

    /// <summary>
    /// A handle to Python threads
    /// </summary>
    private readonly IntPtr _threadsPtr;

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
        RunPythonAction(HotspotDetectionModule, module => { module.run(eventListener.ToPython()); });
    }

    /// <inheritdoc />
    public void StopHotspotDetection()
    {
        RunPythonAction(HotspotDetectionModule, module => { module.stop_hotspot_detection(); });
    }

    /// <inheritdoc />
    public void CalibrateCamera()
    {
        //TODO Change to the actual entrypoint
        RunPythonAction(CalibrationModule, module => { module.test(); });
    }

    /// <summary>
    /// Runs the given action after acquiring the Python GIL importing the given module
    /// </summary>
    /// <param name="moduleName">The name of the Python module to import</param>
    /// <param name="action">The action to run using the Python module</param>
    private static void RunPythonAction(string moduleName, Action<dynamic> action)
    {
        using (Py.GIL())
        {
            //TODO Import a module once and reuse it
            dynamic module = Py.Import($"{ScriptPath}.{moduleName}");
            action(module);
        }
    }

    public void Dispose()
    {
        PythonEngine.EndAllowThreads(_threadsPtr);
        PythonEngine.Shutdown();
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
    public void StopHotspotDetection()
    {
        Console.WriteLine("Stopping hotspot detection");
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
