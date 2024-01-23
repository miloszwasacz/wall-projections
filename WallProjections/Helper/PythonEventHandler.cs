using System;
using WallProjections.Helper.Interfaces;

namespace WallProjections.Helper;

/// <summary>
/// The event handler singleton for Python interop
/// </summary>
public class PythonEventHandler : IPythonEventHandler
{
    /// <summary>
    /// The backing field for <see cref="Instance" />
    /// </summary>
    private static PythonEventHandler? _instance;

    /// <summary>
    /// The global instance of the event handler
    /// </summary>
    /// <remarks>If possible, don't use this global instance - use Dependency Injection instead</remarks>
    public static PythonEventHandler Instance => _instance ??= new PythonEventHandler();

    private PythonEventHandler()
    {
    }

    /// <inheritdoc />
    public event EventHandler<IPythonEventHandler.HotspotSelectedArgs>? HotspotSelected;

    /// <inheritdoc />
    public void OnPressDetected(int id)
    {
        HotspotSelected?.Invoke(this, new IPythonEventHandler.HotspotSelectedArgs(id));
    }
}
