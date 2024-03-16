using System.Collections.Immutable;

namespace WallProjections.ViewModels.Interfaces.SecondaryScreens;

/// <summary>
/// A viewmodel for projecting hotspots onto the artifact
/// </summary>
public abstract class AbsHotspotDisplayViewModel : ViewModelBase
{
    /// <summary>
    /// A list of <see cref="IHotspotProjectionViewModel">projections</see> of hotspots to be displayed
    /// </summary>
    public abstract ImmutableList<IHotspotProjectionViewModel> Projections { get; }

    /// <summary>
    /// Decides whether or not to display the hotspots
    /// </summary>
    public abstract bool IsVisible { get; protected set; }

    /// <summary>
    /// Changes the <see cref="IHotspotProjectionViewModel.IsActive"/> parameter for all
    /// <see cref="IHotspotProjectionViewModel"/> to false in <see cref="Projections"/>
    /// and sets the <see cref="IHotspotProjectionViewModel.IsActive"/> parameter for <see cref="IHotspotProjectionViewModel"/>
    /// to true in <see cref="Projections"/> where <see cref="IHotspotProjectionViewModel.Id"/> matches
    /// the id passed into the function
    /// </summary>
    /// <param name="id">The id of the hotspot to be activated</param>
    public abstract void ActivateHotspot(int id);

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
