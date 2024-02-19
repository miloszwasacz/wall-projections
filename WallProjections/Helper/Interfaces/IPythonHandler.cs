using System;
using System.Threading.Tasks;

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
    public Task RunCalibration();

    /// <summary>
    /// Stops the currently running Python task, if any
    /// </summary>
    public void CancelCurrentTask();

    /// <summary>
    /// Called by Python when a hotspot press is detected
    /// </summary>
    /// <param name="id">The ID of the pressed hotspot</param>
    public void OnPressDetected(int id);

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
