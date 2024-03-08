using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.Test.Mocks.ViewModels.Display.Layouts;

public class MockGenericLayout : Layout, IDisposable
{
    /// <summary>
    /// Whether <see cref="Dispose" /> has been called.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// The media to display in the layout.
    /// </summary>
    public Hotspot.Media Media { get; }

    public MockGenericLayout(Hotspot.Media media)
    {
        Media = media;
    }

    public void Dispose()
    {
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
