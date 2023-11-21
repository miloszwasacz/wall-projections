﻿using System;
using WallProjections.Helper;
using WallProjections.Models;
using WallProjections.Models.Interfaces;

namespace WallProjections.ViewModels.Interfaces;

public interface IDisplayViewModel : IDisposable
{
    /// <summary>
    /// The currently set <see cref="IConfig" /> used to fetch <see cref="Hotspot" />s
    /// </summary>
    public IConfig Config { set; }

    /// <summary>
    /// Determines whether the <see cref="Config" /> has been properly loaded
    /// </summary>
    public bool IsContentLoaded { get; }

    /// <summary>
    /// The description of the currently selected <see cref="Hotspot" />
    /// </summary>
    public string Description { get; }

    //TODO Change to an interface
    /// <summary>
    /// The <see cref="ImageViewModel" /> used to display images
    /// </summary>
    public ImageViewModel ImageViewModel { get; }

    /// <summary>
    /// The <see cref="IVideoViewModel" /> used to display videos
    /// </summary>
    public IVideoViewModel VideoViewModel { get; }

    /// <summary>
    /// Event callback for when a <see cref="Hotspot" /> is selected
    /// </summary>
    /// <param name="sender">The sender of the event</param>
    /// <param name="e">Event args holding the ID of the selected <see cref="Hotspot" /></param>
    public void OnHotspotSelected(object? sender, PythonEventHandler.HotspotSelectedArgs e);
}
