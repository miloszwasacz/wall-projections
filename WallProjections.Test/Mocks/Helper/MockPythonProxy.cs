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

    /// <summary>
    /// A delay in milliseconds applied to every operation to simulate the Python runtime
    /// </summary>
    public int Delay { get; set; }

    /// <summary>
    /// The exception to throw when an operation is called
    /// </summary>
    public Exception? Exception { get; set; }

    public void StartHotspotDetection(IPythonHandler eventListener)
    {
        Task.Delay(Delay).Wait();
        IsHotspotDetectionRunning = true;
        if (Exception != null)
            throw Exception;
    }

    public void StopHotspotDetection()
    {
        IsHotspotDetectionRunning = false;
        Task.Delay(Delay).Wait();
    }

    public void CalibrateCamera()
    {
        Task.Delay(Delay).Wait();
        if (Exception != null)
            throw Exception;

        IsCameraCalibrated = true;
    }

    public void Dispose()
    {
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
