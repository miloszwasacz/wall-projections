using System;
using System.Collections.Generic;
using ReactiveUI;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces;
using WallProjections.Models.Interfaces;

namespace WallProjections.ViewModels;

public class HotspotViewModel : ViewModelBase, IHotspotViewModel
{
    private List<Coord> _hotspots;
    private readonly IConfig? _config;
    
    public HotspotViewModel()
    {
        _config = new Config(
            hotspots: new List<Hotspot>
            {
                new(id: 1, x: 100, y: 100, r: 50),
                new(id: 2, x: 300, y: 200, r: 100)
            });
        _hotspots = new List<Coord>();
        GetFirstHotspot();
    }
    
    public List<Coord> Coordinates
    {
        get => _hotspots;
        private set
        {
            this.RaiseAndSetIfChanged(ref _hotspots, value);
        }
    }

    private void GetFirstHotspot()
    {
        var hotspot = _config?.GetHotspot(1);
        if (hotspot is not null)
        {
            _hotspots.Add(hotspot.Position);
        }
    }
}
