using System.Collections.Generic;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces;
using WallProjections.Models.Interfaces;

namespace WallProjections.ViewModels;

public class HotspotViewModel : ViewModelBase, IHotspotViewModel
{
    private readonly IConfig? _config;
    
    public HotspotViewModel()
    {
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
        DisplayHotspots();
        // ActivateHotspot(2);
    }

    /// <inheritdoc/>
    public List<HotCoord> Coordinates { get; private set; }
    
    /// <inheritdoc/>
    public bool ShowHotspots { get; private set; }
    
    private List<HotCoord> GetHotspots()
    {
        var coord = new List<HotCoord>();
        foreach (var hotspot in _config!.Hotspots)
        {
            var pos = hotspot.Position;
            var hotCord = new HotCoord(hotspot.Id, pos.X, pos.Y, pos.R, pos.R * 2, false);
            coord.Add(hotCord);
        }
        return coord;
    }

    /// <inheritdoc/>
    public void ActivateHotspot(int id)
    {
        var updatedCoords = new List<HotCoord>();
        foreach (var coord in Coordinates)
        {
            if (coord.Vis) {
                var newCoord = new HotCoord(coord.Id, coord.X, coord.Y, coord.R, coord.D, false);
                updatedCoords.Add(newCoord);
            } 
            else if (coord.Id == id) 
            {
                var newCoord = new HotCoord(coord.Id, coord.X, coord.Y, coord.R, coord.D, true);
                updatedCoords.Add(newCoord);
            } 
            else {
                updatedCoords.Add(coord);
            }
        }
        Coordinates = updatedCoords;
    }

    /// <inheritdoc/>
    public void DisplayHotspots()
    {
        ShowHotspots = true;
    }
}
