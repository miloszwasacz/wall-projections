using System.Collections.Generic;

namespace WallProjections.ViewModels.Interfaces.SecondaryScreens;

/// <summary>
/// A viewmodel for projecting hotspots onto the artifact
/// </summary>
public abstract class AbsHotspotDisplayViewModel : ViewModelBase
{
    /// <summary>
    /// An interator of <see cref="IHotspotProjectionViewModel">projections</see> of hotspots to be displayed
    /// </summary>
    public abstract IEnumerable<IHotspotProjectionViewModel> Projections { get; }

    /// <summary>
    /// Decides whether or not to display the hotspots
    /// </summary>
    public abstract bool IsVisible { get; protected set; }

    /// <summary>
    /// Changes the <see cref="IHotspotProjectionViewModel.IsActive"/> parameter for the
    /// <see cref="IHotspotProjectionViewModel"/> to false for all <see cref="Projections"/>
    /// (to be used when user doesn't hover over hotspot for long enough)
    /// </summary>
    public abstract void DeactivateHotspots();

    /// <summary>
    /// Changes <see cref="IsVisible" /> to true
    /// </summary>
    public abstract void DisplayHotspots();
}
