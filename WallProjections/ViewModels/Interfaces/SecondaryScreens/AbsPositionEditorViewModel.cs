﻿using System;
using System.Collections.Generic;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.ViewModels.Interfaces.SecondaryScreens;

/// <summary>
/// A viewmodel for editing the position of a hotspot.
/// </summary>
public abstract class AbsPositionEditorViewModel : ViewModelBase
{
    /// <summary>
    /// An event that is raised when <see cref="UpdateSelectedHotspot" /> is called.
    /// </summary>
    public abstract event EventHandler? HotspotPositionChanged;

    /// <summary>
    /// Whether the position of the hotspot can be changed.
    /// </summary>
    public abstract bool IsInEditMode { get; set; }

    /// <summary>
    /// The currently selected hotspot in the Editor.
    /// </summary>
    protected abstract IEditorHotspotViewModel? SelectedHotspot { set; }

    /// <summary>
    /// A collection of all hotspots that are not currently selected.
    /// </summary>
    public abstract IEnumerable<Coord> UnselectedHotspots { get; protected set; }

    /// <summary>
    /// The current X position of the hotspot.
    /// </summary>
    /// <remarks>
    /// This is not the same as <see cref="SelectedHotspot" />.<see cref="IEditorHotspotViewModel.Position" />.<see cref="Coord.X" />!
    /// To update the position of the selected hotspot, call <see cref="UpdateSelectedHotspot" />.
    /// </remarks>
    public abstract double X { get; protected set; }

    /// <summary>
    /// The current Y position of the hotspot.
    /// </summary>
    /// <remarks>
    /// This is not the same as <see cref="SelectedHotspot" />.<see cref="IEditorHotspotViewModel.Position" />.<see cref="Coord.Y" />!
    /// To update the position of the selected hotspot, call <see cref="UpdateSelectedHotspot" />.
    /// </remarks>
    public abstract double Y { get; protected set; }

    /// <summary>
    /// The current radius of the hotspot (non-negative).
    /// </summary>
    /// <remarks>
    /// This is not the same as <see cref="SelectedHotspot" />.<see cref="IEditorHotspotViewModel.Position" />.<see cref="Coord.R" />!
    /// To update the position of the selected hotspot, call <see cref="UpdateSelectedHotspot" />.
    /// </remarks>
    public abstract double R { get; protected set; }

    /// <summary>
    /// Selects the given <paramref name="hotspot" /> and updates the <see cref="UnselectedHotspots" />.
    /// </summary>
    /// <param name="hotspot">The hotspot to select.</param>
    /// <param name="unselectedHotspots">All other hotspots that are not selected.</param>
    public void SelectHotspot(IEditorHotspotViewModel? hotspot, IEnumerable<Coord> unselectedHotspots)
    {
        SelectedHotspot = hotspot;
        UnselectedHotspots = unselectedHotspots;
        IsInEditMode = false;
    }

    /// <summary>
    /// Updates <see cref="X" /> and <see cref="Y" /> to the given values.
    /// </summary>
    public abstract void SetPosition(double x, double y);

    /// <summary>
    /// Adds <paramref name="delta" /> to the current radius.
    /// </summary>
    /// <param name="delta">The amount to add to the radius.</param>
    public abstract void ChangeRadius(double delta);

    /// <summary>
    /// Changes the position of the <see cref="SelectedHotspot" /> to the current
    /// <see cref="X" />, <see cref="Y" />, and <see cref="R" /> values.
    /// Then, sets <see cref="IsInEditMode" /> to <i>false</i> and raises the <see cref="HotspotPositionChanged" /> event.
    /// </summary>
    public abstract void UpdateSelectedHotspot();
}
