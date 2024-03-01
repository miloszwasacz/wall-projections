using WallProjections.Helper.Interfaces;

namespace WallProjections.Test.Mocks.Helper;

/// <summary>
/// A mock for <see cref="IPythonProxy" />
/// </summary>
public class MockPythonProxy : IPythonProxy
{
    /// <summary>
    /// The returned result of the <see cref="CalibrateCamera" /> method when the input is not empty
    /// </summary>
    public static readonly float[,] CalibrationResult =
    {
        { 0.0f, 0.1f, 0.2f },
        { 1.0f, 1.1f, 1.2f },
        { 2.0f, 2.1f, 2.2f }
    };

    /// <summary>
    /// Whether <see cref="Dispose" /> has been called
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Whether <see cref="StartHotspotDetection" /> has been called
    /// and not yet <see cref="StopCurrentAction">stopped</see>
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

    public void StopCurrentAction()
    {
        IsHotspotDetectionRunning = false;
        Task.Delay(Delay).Wait();
    }

    public float[,]? CalibrateCamera(Dictionary<int, (float, float)> arucoPositions)
    {
        Task.Delay(Delay).Wait();
        if (Exception != null)
            throw Exception;

        IsCameraCalibrated = true;
        return arucoPositions.Count > 0 ? CalibrationResult : null;
    }

    public void Dispose()
    {
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
