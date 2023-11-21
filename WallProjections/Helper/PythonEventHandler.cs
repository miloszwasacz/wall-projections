using System;

namespace WallProjections.Helper;

/// <summary>
/// The event handler singleton for Python interop
/// </summary>
public class PythonEventHandler
{
    private static PythonEventHandler? _instance;
    public static PythonEventHandler Instance => _instance ??= new PythonEventHandler();

    private PythonEventHandler()
    {
    }

    public event EventHandler<HotspotSelectedArgs>? HotspotSelected;

    public class HotspotSelectedArgs : EventArgs
    {
        public int Id { get; }

        public HotspotSelectedArgs(int id)
        {
            Id = id;
        }
    }

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// Called by Python when a hotspot press is detected
    /// </summary>
    /// <param name="id">The ID of the pressed hotspot</param>
    public void OnPressDetected(int id)
    {
        HotspotSelected?.Invoke(this, new HotspotSelectedArgs(id));
    }
}
