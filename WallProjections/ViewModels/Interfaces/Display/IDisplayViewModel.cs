using System;
using WallProjections.Helper.Interfaces;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.Display.Layouts;
using WallProjections.Views;

namespace WallProjections.ViewModels.Interfaces.Display;

/// <summary>
/// A viewmodel for displaying all the information about a <see cref="Hotspot" />.
/// </summary>
public interface IDisplayViewModel : IDisposable
{
    /// <summary>
    /// Event callback for when a <see cref="Hotspot" /> is selected.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">Event args holding the ID of the selected <see cref="Hotspot" />.</param>
    public void OnHotspotSelected(object? sender, IPythonHandler.HotspotSelectedArgs e);

    /// <summary>
    /// The concrete <see cref="Layout" /> currently being displayed,
    /// based on the selected <see cref="Hotspot" />'s content.
    /// </summary>
    public Layout? ContentViewModel { get; }

    /// <summary>
    /// Opens the <see cref="EditorWindow">Editor</see>.
    /// </summary>
    public void OpenEditor();

    /// <summary>
    /// Closes the <see cref="DisplayWindow">Display</see>.
    /// </summary>
    public void CloseDisplay();
}
