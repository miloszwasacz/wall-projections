using System.Collections.Generic;
using System.Collections.Immutable;
using Avalonia;
using WallProjections.Models;

namespace WallProjections.ViewModels.Interfaces.SecondaryScreens;

// ReSharper disable once InconsistentNaming
/// <summary>
/// A viewmodel for displaying a grid of ArUco markers.
/// </summary>
public abstract class IArUcoGridViewModel : ViewModelBase
{
    /// <summary>
    /// A list of all available ArUco markers.
    /// </summary>
    public abstract ImmutableList<ArUco> ArUcoList { get; }

    /// <summary>
    /// A dictionary of ArUco ids and their positions.
    /// </summary>
    public Dictionary<int, Point>? ArUcoPositions { get; set; }
}
