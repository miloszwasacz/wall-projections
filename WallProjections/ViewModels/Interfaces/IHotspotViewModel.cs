using System.Collections.Generic;
using WallProjections.Models;

namespace WallProjections.ViewModels.Interfaces;

public interface IHotspotViewModel
{
    public List<HotCoord> Coordinates { get; }
    public void ActivateHotspot(int id);
}
