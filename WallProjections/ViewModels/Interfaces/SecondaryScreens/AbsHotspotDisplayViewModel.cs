using System.Collections.Generic;

namespace WallProjections.ViewModels.Interfaces.SecondaryScreens;

/// <summary>
/// A viewmodel for projecting hotspots onto the artifact
/// </summary>
public abstract class AbsHotspotDisplayViewModel : ViewModelBase
{
    /// <summary>
    /// An iterator of <see cref="AbsHotspotProjectionViewModel">projections</see> of hotspots to be displayed
    /// </summary>
    public abstract IEnumerable<AbsHotspotProjectionViewModel> Projections { get; }

    /// <summary>
    /// Whether to display the hotspots
    /// </summary>
    public abstract bool IsVisible { get; protected set; }

    /// <summary>
    /// Changes the <see cref="AbsHotspotProjectionViewModel.State" /> of all
    /// <see cref="AbsHotspotProjectionViewModel" />s in <see cref="Projections" />
    /// to <see cref="HotspotState.None" />
    /// (to be used when user doesn't hover over hotspot for long enough)
    /// </summary>
    public abstract void DeactivateHotspots();

    /// <summary>
    /// Changes <see cref="IsVisible" /> to true
    /// </summary>
    public abstract void DisplayHotspots();
}
