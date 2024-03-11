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
    /// Gets a simple layout with a title and description.
    /// </summary>
    /// <param name="title">Title displayed by the layout.</param>
    /// <param name="description">Description displayed by the layout.</param>
    public Layout GetSimpleDescriptionLayout(string title, string description);

    /// <summary>
    /// Gets a layout with an error message.
    /// </summary>
    /// <param name="message">Error message to display.</param>
    /// <param name="title">Title displayed by the layout.</param>
    public Layout GetErrorLayout(string message, string title = DefaultErrorTitle) =>
        GetSimpleDescriptionLayout(title, message);
}
