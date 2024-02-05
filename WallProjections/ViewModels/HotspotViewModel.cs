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
                new(id: 1, x: 100, y: 100, r: 70),
                new(id: 2, x: 300, y: 200, r: 40),
                new(id: 3, x: 40, y: 400, r: 80)
            });
        Coordinates = GetHotspots();
    }
    
    public List<HotCoord> Coordinates { get; }

    private List<HotCoord> GetHotspots()
    {
        var coord = new List<HotCoord>();
        for (var i = 0; i < _config?.HotspotCount; i++)
        {
            var hotspot = _config?.GetHotspot(i + 1);
            if (hotspot is null) continue;
            var pos = hotspot.Position;
            var hotCord = new HotCoord(pos.X, pos.Y, pos.R, pos.R * 2);
            coord.Add(hotCord);
        }

        return coord;
    }
}
