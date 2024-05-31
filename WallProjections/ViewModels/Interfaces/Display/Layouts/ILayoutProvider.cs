using WallProjections.Models;

namespace WallProjections.ViewModels.Interfaces.Display.Layouts;

/// <summary>
/// A provider for creating layouts for hotspots.
/// </summary>
public interface ILayoutProvider
{
    /// <summary>
    /// The default title for an <see cref="GetErrorLayout">error layout</see>.
    /// </summary>
    public const string DefaultErrorTitle = "Error";

    /// <summary>
    /// Gets a compatible layout for the hotspot or a layout with an error message.
    /// </summary>
    /// <param name="vmProvider"><see cref="IViewModelProvider"/> to use to produce the internal viewmodels.</param>
    /// <param name="hotspot"><see cref="Hotspot.Media"/> to create layout with.</param>
    /// <returns>Appropriate layout for the hotspot or a layout with an error message.</returns>
    public Layout GetLayout(IViewModelProvider vmProvider, Hotspot.Media hotspot);

    /// <summary>
    /// Gets a layout with a welcome message.
    /// </summary>
    public Layout GetWelcomeLayout();

    /// <summary>
    /// Gets a layout with a message.
    /// </summary>
    /// <param name="title">The message title.</param>
    /// <param name="description">The full description.</param>
    public Layout GetMessageLayout(string title, string description);

    /// <summary>
    /// Gets a layout with an error message.
    /// </summary>
    /// <param name="message">Error message to display.</param>
    /// <param name="title">Title displayed by the layout.</param>
    public Layout GetErrorLayout(string message, string title = DefaultErrorTitle);
}
