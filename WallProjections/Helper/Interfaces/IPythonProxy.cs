using System;
using System.Collections.Immutable;
using Avalonia;
using WallProjections.Models;
using WallProjections.Models.Interfaces;

namespace WallProjections.Helper.Interfaces;

/// <summary>
/// A proxy for setting up Python runtime and executing Python scripts
/// </summary>
public interface IPythonProxy : IDisposable
{
    /// <inheritdoc cref="PythonModule.HotspotDetectionModule.StartDetection" />
    public void StartHotspotDetection(IPythonHandler eventListener, IConfig config);

    /// <summary>
    /// Tells Python to stop the currently running action
    /// </summary>
    public void StopCurrentAction();

    /// <inheritdoc cref="PythonModule.CalibrationModule.CalibrateCamera" />
    public double[,]? CalibrateCamera(int cameraIndex, ImmutableDictionary<int, Point> arucoPositions);

    /// <inheritdoc cref="PythonModule.CameraIdentificationModule.GetAvailableCameras" />
    public ImmutableList<Camera> GetAvailableCameras();
}
