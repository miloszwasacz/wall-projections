using System;
using WallProjections.Helper.Interfaces;
using WallProjections.Models;

namespace WallProjections.ViewModels.Interfaces.Display;

/// <summary>
/// A viewmodel for displaying all the information about a <see cref="Hotspot" />
/// </summary>
public interface IDisplayViewModel : IDisposable
{
    /// <summary>
    /// The description of the currently selected <see cref="Hotspot" />
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// The <see cref="ImageViewModel" /> used to display images
    /// </summary>
    public IImageViewModel ImageViewModel { get; }

    /// <summary>
    /// The <see cref="IVideoViewModel" /> used to display videos
    /// </summary>
    public IVideoViewModel VideoViewModel { get; }

    /// <summary>
    /// Event callback for when a <see cref="Hotspot" /> is selected
    /// </summary>
    /// <param name="sender">The sender of the event</param>
    /// <param name="e">Event args holding the ID of the selected <see cref="Hotspot" /></param>
    public void OnHotspotSelected(object? sender, IPythonEventHandler.HotspotSelectedArgs e);
}
