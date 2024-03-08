using WallProjections.Models;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.Test.Mocks.ViewModels.Display.Layouts;

public class MockLayoutProvider : ILayoutProvider
{
    /// <returns>A new <see cref="MockGenericLayout" /> with the given media.</returns>
    public Layout GetLayout(IViewModelProvider vmProvider, Hotspot.Media hotspot) =>
        new MockGenericLayout(hotspot);

    /// <returns>A new <see cref="MockSimpleDescriptionLayout" /> with the given title and description.</returns>
    public Layout GetSimpleDescriptionLayout(string title, string description) =>
        new MockSimpleDescriptionLayout(title, description);
}
