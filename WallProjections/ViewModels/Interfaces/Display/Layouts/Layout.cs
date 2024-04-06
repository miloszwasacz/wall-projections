namespace WallProjections.ViewModels.Interfaces.Display.Layouts;

/// <summary>
/// A base class for all available layout types for display content about a hotspot
/// </summary>
public abstract class Layout : ViewModelBase
{
    /// <summary>
    /// The id of the hotspot associated with this layout.
    /// </summary>
    public int? HotspotId { get; }

    /// <summary>
    /// Creates a new <see cref="Layout"/> with the given <paramref name="hotspotId"/>.
    /// </summary>
    /// <param name="hotspotId">
    /// The id of the hotspot associated with this layout, or <i>null"</i> if there is no associated hotspot.
    /// </param>
    protected Layout(int? hotspotId)
    {
        HotspotId = hotspotId;
    }
}
