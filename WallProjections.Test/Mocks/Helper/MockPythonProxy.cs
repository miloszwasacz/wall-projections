using System.Collections.Immutable;
using Avalonia;
using WallProjections.Helper.Interfaces;
using WallProjections.Models;
using WallProjections.Models.Interfaces;

namespace WallProjections.Test.Mocks.Helper;

/// <summary>
/// A mock for <see cref="IPythonProxy" />
/// </summary>
public class MockPythonProxy : IPythonProxy
{
    /// <summary>
    /// The returned result of the <see cref="CalibrateCamera" /> method when the input is not empty
    /// </summary>
    public static readonly double[,] CalibrationResult =
    {
        { 0.0d, 0.1d, 0.2d },
        { 1.0d, 1.1d, 1.2d },
        { 2.0d, 2.1d, 2.2d }
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

    public void StartHotspotDetection(IPythonHandler eventListener, IConfig config)
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

    public double[,]? CalibrateCamera(int cameraIndex, ImmutableDictionary<int, Point> arucoPositions)
    {
        Task.Delay(Delay).Wait();
        if (Exception != null)
            throw Exception;

        IsCameraCalibrated = true;
        return arucoPositions.Count > 0 ? CalibrationResult : null;
    }

    public ImmutableList<Camera> GetAvailableCameras()
    {
        Task.Delay(Delay).Wait();
        if (Exception != null)
            throw Exception;

        return ImmutableList.Create(
            new Camera(0, "Camera 0"),
            new Camera(700, "Camera 1"),
            new Camera(702, "Camera 2")
        );
    }

    public void Dispose()
    {
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
