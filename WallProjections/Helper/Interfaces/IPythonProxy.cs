using System;

namespace WallProjections.Helper.Interfaces;

/// <summary>
/// A proxy for setting up Python runtime and executing Python scripts
/// </summary>
public interface IPythonProxy : IDisposable
{
    /// <summary>
    /// Starts the detection of hotspots using computer vision
    /// </summary>
    /// <param name="eventListener">The event listener to notify when a hotspot press is detected</param>
    public void StartHotspotDetection(IPythonHandler eventListener);

    /// <summary>
    /// Stops the detection of hotspots
    /// </summary>
    public void StopHotspotDetection();

    /// <summary>
    /// Calibrates the camera
    /// </summary>
    public void CalibrateCamera();
}
