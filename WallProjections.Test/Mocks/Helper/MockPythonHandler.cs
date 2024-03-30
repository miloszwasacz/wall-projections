using System.Collections.Immutable;
using Avalonia;
using WallProjections.Helper;
using WallProjections.Helper.Interfaces;
using WallProjections.Models.Interfaces;

namespace WallProjections.Test.Mocks.Helper;

/// <summary>
/// A mock of <see cref="PythonHandler" />
/// </summary>
/// <remarks>
/// This is basically the same thing as <see cref="PythonHandler" />,
/// but with a public constructor instead of being a singleton
/// </remarks>
public class MockPythonHandler : IPythonHandler
{
    public event EventHandler<IHotspotHandler.HotspotArgs>? HotspotPressed;
    public event EventHandler<IHotspotHandler.HotspotArgs>? HotspotReleased;

    /// <summary>
    /// The delay (in milliseconds) simulating the time it takes to call a Python script
    /// </summary>
    public int Delay { get; set; }

    public async Task RunHotspotDetection(IConfig config)
    {
        CurrentScript = PythonScript.HotspotDetection;
        await Task.Delay(Delay);
    }

    public async Task<double[,]?> RunCalibration(ImmutableDictionary<int, Point> arucoPositions)
    {
        CurrentScript = PythonScript.Calibration;
        await Task.Delay(Delay);
        return arucoPositions.Count > 0 ? MockPythonProxy.CalibrationResult : null;
    }

    public void CancelCurrentTask()
    {
        CurrentScript = null;
    }

    /// <summary>
    /// The current script being run
    /// </summary>
    /// <seealso cref="PythonScript" />
    public PythonScript? CurrentScript { get; private set; }

    /// <summary>
    /// Whether <see cref="Dispose" /> has been called
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <inheritdoc />
    public void OnHotspotPressed(int id)
    {
        HotspotPressed?.Invoke(this, new IHotspotHandler.HotspotArgs(id));
    }

    /// <inheritdoc />
    public void OnHotspotUnpressed(int id)
    {
        HotspotReleased?.Invoke(this, new IHotspotHandler.HotspotArgs(id));
    }

    /// <summary>
    /// Returns if there are any subscribers to <see cref="HotspotPressed" />
    /// </summary>
    public bool HasPressedSubscribers => HotspotPressed is not null && HotspotPressed.GetInvocationList().Length > 0;

    /// <summary>
    /// Returns if there are any subscribers to <see cref="HotspotReleased" />
    /// </summary>
    public bool HasReleasedSubscribers => HotspotReleased is not null && HotspotReleased.GetInvocationList().Length > 0;

    public void Dispose()
    {
        IsDisposed = true;
        CancelCurrentTask();
        GC.SuppressFinalize(this);
    }

    public enum PythonScript
    {
        HotspotDetection,
        Calibration
    }
}
