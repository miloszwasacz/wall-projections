#if !DEBUGSKIPPYTHON

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using Avalonia;
using Microsoft.Extensions.Logging;
using Python.Runtime;
using WallProjections.Helper.Interfaces;
using WallProjections.Models;
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

    /// <inheritdoc cref="CameraIdentificationModule" />
    public static Func<CameraIdentificationModule> CameraIdentification => () => new CameraIdentificationModule();

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

            _rawModule.hotspot_detection(
                eventListener.CameraIndex,
                eventListener.ToPython(),
                config.HomographyMatrix,
                serialized
            );
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
        /// <param name="cameraIndex">The index of the camera that will be passed to OpenCV</param>
        /// <param name="arucoPositions">The positions of the ArUco markers (ID, top-left corner)</param>
        public double[,]? CalibrateCamera(int cameraIndex, ImmutableDictionary<int, Point> arucoPositions)
        {
            var positions = arucoPositions.ToDictionary(
                pair => pair.Key,
                pair => new[] { pair.Value.X, pair.Value.Y }
            );

            try
            {
                var serialized = JsonSerializer.Serialize(positions);
                PyObject? homography = _rawModule.calibrate(cameraIndex, serialized);
                return homography?.As<double[,]>();
            }
            catch (Exception e)
            {
                //TODO Refactor to use a custom exceptions based on the reason for the failure
                throw new Exception($"Failed to calibrate the camera: {e.Message}");
            }
        }
    }

    /// <summary>
    /// A module that identifies the available cameras on the system that OpenCV can access
    /// </summary>
    public sealed class CameraIdentificationModule : PythonModule
    {
        public CameraIdentificationModule() : base("camera_identifier")
        {
        }

        /// <summary>
        /// Identifies the available cameras on the system that OpenCV can access
        /// </summary>
        /// <returns>A dictionary of camera indices (passed to OpenCV) and their corresponding names</returns>
        public ImmutableList<Camera> GetAvailableCameras(ILogger logger)
        {
            PyObject? camerasPy = _rawModule.get_cameras();
            var serialized = camerasPy?.As<string>();
            if (serialized is null)
            {
                logger.LogError("Failed to get cameras from Python");
                return ImmutableList<Camera>.Empty;
            }

            try
            {
                var cameras = JsonSerializer.Deserialize<ImmutableDictionary<int, string>>(serialized);
                if (cameras is null) throw new NullReferenceException("Deserialized cameras is null");

                logger.LogTrace("Cameras detected by Python: {Cameras}", serialized);
                return cameras.Select(pair => new Camera(pair.Key, pair.Value)).ToImmutableList();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to deserialize cameras");
                return ImmutableList<Camera>.Empty;
            }
        }
    }
}

#endif
