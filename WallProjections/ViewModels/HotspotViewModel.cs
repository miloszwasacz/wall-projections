using System.Collections.Generic;
using System.Linq;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces;
using WallProjections.Models.Interfaces;

namespace WallProjections.ViewModels;

public class HotspotViewModel : ViewModelBase, IHotspotViewModel
{
    private readonly IConfig? _config;
    
    public HotspotViewModel()
    {
        //mock data for _config, change to real once editor complete
        _config = new Config(
            hotspots: new List<Hotspot>
            {
                new(id: 0, x: 100, y: 100, r: 30),
                new(id: 1, x: 300, y: 200, r: 40),
                new(id: 2, x: 700, y: 210, r: 35),
                new(id: 3, x: 650, y: 400, r: 45),
                new(id: 4, x: 40, y: 400, r: 45)
            });
        Coordinates = GetHotspots();
        // DisplayHotspots();
        // ActivateHotspot(2);
        // DeactivateHotspots();
    }

    /// <inheritdoc/>
    public List<HotspotProjection> Coordinates { get; private set; }
    
    /// <inheritdoc/>
    public bool IsVisible { get; private set; }
    
    /// <summary>
    /// Goes through all the hotspots in the config file and turns them into <see cref="HotspotProjection"/>
    /// instances 
    /// </summary>
    /// <returns>List of <see cref="HotspotProjection"/> relating to all hotspots in config file</returns>
    private List<HotspotProjection> GetHotspots()
    {
        var hotspots = from hotspot in _config!.Hotspots
            let pos = hotspot.Position
            select new HotspotProjection
            {
                Id = hotspot.Id,
                X = pos.X,
                Y = pos.Y,
                D = pos.R * 2,
                IsActive = false
            };
        var hotspotList = hotspots.ToList();
        return hotspotList;
    }

    /// <inheritdoc/>
    public void ActivateHotspot(int id)
    {
        foreach (var coord in Coordinates)
        {
            if (coord.Id == id)
            {
                coord.IsActive = true;
            } 
            else if (coord.IsActive)
            {
                coord.IsActive = false;
            } 
        }
    }

    /// <inheritdoc/>
    public void DeactivateHotspots()
    {
        foreach (var coord in Coordinates)
        {
            if (coord.IsActive)
            {
                coord.IsActive = false;
            } 
        }
    }

    /// <inheritdoc/>
    public void DisplayHotspots()
    {
        IsVisible = true;
    }
}
