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
                new(id: 1, x: 100, y: 100, r: 50),
                new(id: 2, x: 300, y: 200, r: 100),
                new(id: 3, x: 40, y: 400, r: 150)
            });
        Coordinates = GetHotspots();
    }
    
    public List<Coord> Coordinates { get; }

    private List<Coord> GetHotspots()
    {
        var coord = new List<Coord>();
        for (int i = 0; i < _config?.HotspotCount; i++)
        {
            var hotspot = _config?.GetHotspot(i + 1);
            if (hotspot is not null)
            {
                coord.Add(hotspot.Position);
            }
        }

        return coord;
    }
}
