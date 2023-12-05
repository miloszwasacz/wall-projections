using System;

namespace WallProjections.Helper.Interfaces;

public interface IPythonEventHandler
{
    /// <summary>
    /// An event that is fired when a hotspot is pressed
    /// </summary>
    public event EventHandler<HotspotSelectedArgs>? HotspotSelected;

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
