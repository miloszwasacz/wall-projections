using System;
using WallProjections.Models.Interfaces;

namespace WallProjections.ViewModels.Interfaces;

public interface IDisplayViewModel : IDisposable
{
    public IConfig Config { set; }

    public string Description { get; }

    //TODO Change to an interface
    public ImageViewModel ImageViewModel { get; }

    public IVideoViewModel VideoViewModel { get; }

    public bool LoadHotspot(int hotspotId);
}
