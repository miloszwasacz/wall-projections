using System.Collections.Immutable;
using WallProjections.Models;

namespace WallProjections.ViewModels.Interfaces.SecondaryScreens;

/// <summary>
/// A viewmodel for displaying a grid of ArUco markers.
/// </summary>
public abstract class AbsArUcoGridViewModel : ViewModelBase
{
    /// <summary>
    /// A list of all available ArUco markers.
    /// </summary>
    public abstract ImmutableList<ArUco> ArUcoList { get; }
}
