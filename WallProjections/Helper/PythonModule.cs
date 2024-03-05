#if !DEBUGSKIPPYTHON

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using Avalonia;
using Python.Runtime;
using WallProjections.Helper.Interfaces;
using WallProjections.Models.Interfaces;

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

        /// <summary>
        /// Starts the detection of hotspots using computer vision
        /// </summary>
        /// <param name="eventListener">The event listener to notify when a hotspot press is detected</param>
        /// <param name="config">The config holding the calibration matrix and hotspot positions</param>
        public void StartDetection(IPythonHandler eventListener, IConfig config)
        {
            var positions = config.Hotspots.ToDictionary(
                hotspot => hotspot.Id,
                hotspot => new[] { hotspot.Position.X, hotspot.Position.Y, hotspot.Position.R }
            );
            var serialized = JsonSerializer.Serialize(positions);

            _rawModule.hotspot_detection(eventListener.ToPython(), config.HomographyMatrix, serialized);
        }

        /// <summary>
        /// Stops the detection of hotspots (if it is currently running)
        /// </summary>
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

        /// <summary>
        /// Calibrates the camera and returns the homography matrix
        /// </summary>
        /// <param name="arucoPositions">The positions of the ArUco markers (ID, top-left corner)</param>
        public double[,]? CalibrateCamera(ImmutableDictionary<int, Point> arucoPositions)
        {
            var positions = arucoPositions.ToDictionary(
                pair => pair.Key,
                pair => new[] { pair.Value.X, pair.Value.Y }
            );

            var serialized = JsonSerializer.Serialize(positions);
            PyObject? homography = _rawModule.calibrate(serialized);
            return homography?.As<double[,]>();
        }
    }
}

#endif
