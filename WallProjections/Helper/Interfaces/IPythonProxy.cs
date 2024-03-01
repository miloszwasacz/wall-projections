using System;
using System.Collections.Generic;

namespace WallProjections.Helper.Interfaces;

/// <summary>
/// A proxy for setting up Python runtime and executing Python scripts
/// </summary>
public interface IPythonProxy : IDisposable
{
    /// <inheritdoc cref="PythonModule.HotspotDetectionModule.StartDetection" />
    public void StartHotspotDetection(IPythonHandler eventListener);

    /// <summary>
    /// Tells Python to stop the currently running action
    /// </summary>
    public void StopCurrentAction();

    /// <inheritdoc cref="PythonModule.CalibrationModule.CalibrateCamera" />
    public float[,]? CalibrateCamera(Dictionary<int, (float, float)> arucoPositions);
}
