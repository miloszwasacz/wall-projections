using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Avalonia;
using WallProjections.Models.Interfaces;

namespace WallProjections.Helper.Interfaces;

public interface IPythonHandler : IDisposable
{
    /// <summary>
    /// An event that is fired when a hotspot is pressed
    /// </summary>
    public event EventHandler<IHotspotHandler.HotspotArgs>? HotspotPressed;

    /// <summary>
    /// An event that is fired when a hotspot is released
    /// </summary>
    public event EventHandler<IHotspotHandler.HotspotArgs>? HotspotReleased;

    /// <summary>
    /// Starts an asynchronous Python task that listens for hotspot presses
    /// </summary>
    public Task RunHotspotDetection(IConfig config);

    /// <summary>
    /// Starts an asynchronous Python task that calibrates the camera
    /// </summary>
    /// <param name="arucoPositions">The positions of the ArUco markers (ID, top-left corner)</param>
    public Task<double[,]?> RunCalibration(ImmutableDictionary<int, Point> arucoPositions);

    /// <summary>
    /// Stops the currently running Python task, if any
    /// </summary>
    public void CancelCurrentTask();

    /// <summary>
    /// Called by Python when a hotspot press is detected
    /// </summary>
    /// <param name="id">The ID of the pressed hotspot</param>
    public void OnHotspotPressed(int id);

    /// <summary>
    /// Called by Python when a hotspot press is released
    /// </summary>
    /// <param name="id">The ID of the released hotspot</param>
    public void OnHotspotUnpressed(int id);
}
