using System.Collections.Generic;
using WallProjections.Models;

namespace WallProjections.ViewModels.Interfaces;

public interface IHotspotViewModel
{
    public List<HotCoord> Coordinates { get; }

    public bool ShowHotspots { get; }
    public void ActivateHotspot(int id);

    public void DisplayHotspots();
}
