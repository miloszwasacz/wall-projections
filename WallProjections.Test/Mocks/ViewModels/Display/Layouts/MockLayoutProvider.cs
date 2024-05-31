using WallProjections.Models;
using WallProjections.ViewModels.Display.Layouts;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.Test.Mocks.ViewModels.Display.Layouts;

public class MockLayoutProvider : ILayoutProvider
{
    /// <returns>A new <see cref="MockGenericLayout" /> with the given media.</returns>
    public Layout GetLayout(IViewModelProvider vmProvider, Hotspot.Media hotspot) =>
        new MockGenericLayout(hotspot);

    /// <returns>
    /// A new <see cref="MockSimpleDescriptionLayout" /> with "Welcome" and "Test" as the title and description.
    /// </returns>
    public Layout GetWelcomeLayout() =>
        new MockSimpleDescriptionLayout(WelcomeViewModel.WelcomeTitle, WelcomeViewModel.WelcomeMessage);

    /// <returns>
    /// A new <see cref="MockSimpleDescriptionLayout" /> with the given title and description.
    /// </returns>
    public Layout GetMessageLayout(string title, string description) =>
        new MockSimpleDescriptionLayout(title, description);

    /// <returns>A new <see cref="MockSimpleDescriptionLayout" /> with the given title and message.</returns>
    public Layout GetErrorLayout(string message, string title = ILayoutProvider.DefaultErrorTitle) =>
        new MockSimpleDescriptionLayout(title, message);
}
