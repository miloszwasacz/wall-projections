using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Avalonia;

namespace WallProjections.Helper.Interfaces;

public interface IPythonHandler : IDisposable
{
    /// <summary>
    /// An event that is fired when a hotspot is pressed
    /// </summary>
    public event EventHandler<HotspotSelectedArgs>? HotspotSelected;

    /// <summary>
    /// Starts an asynchronous Python task that listens for hotspot presses
    /// </summary>
    public Task RunHotspotDetection();

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

    /// <summary>
    /// Arguments for the <see cref="HotspotSelected" /> event
    /// </summary>
    public class HotspotSelectedArgs : EventArgs
    {
        public int Id { get; }

        public HotspotSelectedArgs(int id)
        {
            Id = id;
        }
    }
}
