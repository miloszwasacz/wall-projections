#if !DEBUGSKIPPYTHON

using System;
using Python.Runtime;
using WallProjections.Helper.Interfaces;

namespace WallProjections.Helper;

/// <summary>
/// A container class for all available Python modules.
/// </summary>
public abstract class PythonModule
{
    /// <summary>
    /// The base for importing Python modules
    /// </summary>
    /// <seealso cref="PythonModule(string)" />
    private const string ScriptPath = "Scripts";

    /// <inheritdoc cref="HotspotDetectionModule" />
    public static Func<HotspotDetectionModule> HotspotDetection => () => new HotspotDetectionModule();

    /// <inheritdoc cref="CalibrationModule" />
    public static Func<CalibrationModule> Calibration => () => new CalibrationModule();

    /// <summary>
    /// The Python module object that this class wraps.
    /// This is a dynamic object and can cause runtime errors if used incorrectly.
    /// <b>USE WITH CAUTION!</b>
    /// </summary>
    private readonly dynamic _rawModule;

    /// <summary>
    /// <see cref="Py.Import">Imports</see> the Python module with the given <paramref name="name" />
    /// using the <see cref="ScriptPath" /> as the base (i.e. <i>{ScriptPath}.{name}</i>)
    /// </summary>
    /// <param name="name">The name of the Python module</param>
    private PythonModule(string name)
    {
        _rawModule = Py.Import($"{ScriptPath}.{name}");
    }

    /// <summary>
    /// A module that provides hotspot detection functionality using Computer Vision
    /// </summary>
    public sealed class HotspotDetectionModule : PythonModule
    {
        public HotspotDetectionModule() : base("hotspot_detection")
        {
        }

        public void StartDetection(IPythonHandler eventListener)
        {
            _rawModule.hotspot_detection(eventListener.ToPython());
        }

        public void StopDetection()
        {
            _rawModule.stop_hotspot_detection();
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

        public void Calibrate()
        {
            _rawModule.calibrate();
        }
    }
}

#endif
