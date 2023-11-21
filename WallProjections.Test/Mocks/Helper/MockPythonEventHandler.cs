using WallProjections.Helper;
using WallProjections.Helper.Interfaces;

namespace WallProjections.Test.Mocks.Helper;

/// <summary>
/// A mock of <see cref="PythonEventHandler" />
/// </summary>
/// <remarks>
/// This is basically the same thing as <see cref="PythonEventHandler" />,
/// but with a public constructor instead of being a singleton
/// </remarks>
public class MockPythonEventHandler : IPythonEventHandler
{
    /// <inheritdoc />
    public event EventHandler<IPythonEventHandler.HotspotSelectedArgs>? HotspotSelected;

    /// <inheritdoc />
    public void OnPressDetected(int id)
    {
        HotspotSelected?.Invoke(this, new IPythonEventHandler.HotspotSelectedArgs(id));
    }
}
