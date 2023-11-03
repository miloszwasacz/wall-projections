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

    public event EventHandler<PressDetectedArgs>? PressDetected;

    public class PressDetectedArgs : EventArgs
    {
        public int Button { get; private set; }

        public PressDetectedArgs(int button)
        {
            Button = button;
        }
    }

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// Called by Python when a button press is detected
    /// </summary>
    /// <param name="button">The ID of the pressed button</param>
    public void OnPressDetected(int button)
    {
        PressDetected?.Invoke(this, new PressDetectedArgs(button));
    }
}
