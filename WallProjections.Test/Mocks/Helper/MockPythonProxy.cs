using WallProjections.Helper.Interfaces;

namespace WallProjections.Test.Mocks.Helper;

/// <summary>
/// A mock for <see cref="IPythonProxy" />
/// </summary>
public class MockPythonProxy : IPythonProxy
{
    /// <summary>
    /// Whether <see cref="Dispose" /> has been called
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Whether <see cref="StartHotspotDetection" /> has been called
    /// and not yet <see cref="StopHotspotDetection">stopped</see>
    /// </summary>
    public bool IsHotspotDetectionRunning { get; private set; }

    /// <summary>
    /// Whether <see cref="CalibrateCamera" /> has been called
    /// </summary>
    public bool IsCameraCalibrated { get; private set; }

    public void StartHotspotDetection(IPythonHandler eventListener)
    {
        IsHotspotDetectionRunning = true;
    }

    public void StopHotspotDetection()
    {
        IsHotspotDetectionRunning = false;
    }

    public void CalibrateCamera()
    {
        IsCameraCalibrated = true;
    }

    public void Dispose()
    {
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
