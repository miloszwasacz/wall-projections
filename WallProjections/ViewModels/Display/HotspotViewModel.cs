using System.Collections.Immutable;
using System.Linq;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;

namespace WallProjections.ViewModels.Display;

/// <inheritdoc cref="IHotspotViewModel" />
public class HotspotViewModel : ViewModelBase, IHotspotViewModel
{
    /// <summary>
    /// Creates a new instance of <see cref="HotspotViewModel"/> based on the provided <paramref name="config" />
    /// </summary>
    /// <param name="config">The <see cref="IConfig" /> holding information about the hotspots</param>
    public HotspotViewModel(IConfig config)
    {
        Projections = GetHotspots(config);
    }

    /// <inheritdoc/>
    public ImmutableList<HotspotProjection> Projections { get; }

    /// <inheritdoc/>
    public bool IsVisible { get; private set; }

    /// <inheritdoc/>
    public void ActivateHotspot(int id)
    {
        foreach (var coord in Projections)
        {
            if (coord.Id == id)
                coord.IsActive = true;
            else if (coord.IsActive)
                coord.IsActive = false;
        }
    }

    /// <inheritdoc/>
    public void DeactivateHotspots()
    {
        var active = Projections.Where(coord => coord.IsActive);
        foreach (var coord in active)
            coord.IsActive = false;
    }

    /// <inheritdoc/>
    public void DisplayHotspots()
    {
        IsVisible = true;
    }

    /// <summary>
    /// Goes through all the hotspots in the config file and turns them into
    /// <see cref="HotspotProjection"/> instances
    /// </summary>
    /// <returns>List of <see cref="HotspotProjection"/> relating to all hotspots in config file</returns>
    private static ImmutableList<HotspotProjection> GetHotspots(IConfig config)
    {
        var hotspots = from hotspot in config.Hotspots
            let pos = hotspot.Position
            select new HotspotProjection
            {
                Id = hotspot.Id,
                X = pos.X,
                Y = pos.Y,
                D = pos.R * 2,
                IsActive = false
            };
        return hotspots.ToImmutableList();
    }
}
