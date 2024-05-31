using System;
using System.Threading.Tasks;
using WallProjections.Helper.Interfaces;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.Display.Layouts;
using WallProjections.Views.Display;
using WallProjections.Views.Editor;

namespace WallProjections.ViewModels.Interfaces.Display;

/// <summary>
/// A viewmodel for displaying all the information about a <see cref="Hotspot" />.
/// </summary>
public interface IDisplayViewModel : IDisposable
{
    public static readonly (string Title, string Description) CleanupMessage = (
        "Closing content display",
        "The hand tracking is stopping.\nPlease wait..."
    );

    /// <summary>
    /// Event callback for when a <see cref="Hotspot" /> has been activated.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">Event args holding the ID of the selected <see cref="Hotspot" />.</param>
    public void OnHotspotActivated(object? sender, IHotspotHandler.HotspotArgs e);

    /// <summary>
    /// The concrete <see cref="Layout" /> currently being displayed,
    /// based on the selected <see cref="Hotspot" />'s content.
    /// </summary>
    public Layout ContentViewModel { get; }

    /// <summary>
    /// Opens the <see cref="EditorWindow">Editor</see>.
    /// </summary>
    public Task OpenEditor();

    /// <summary>
    /// Closes the <see cref="DisplayWindow">Display</see>.
    /// </summary>
    public void CloseDisplay();
}
