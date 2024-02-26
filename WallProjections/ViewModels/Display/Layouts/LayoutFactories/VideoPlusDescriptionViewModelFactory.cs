using System;
using System.Linq;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Display.Layouts;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.Models.LayoutFactories;

public class VideoPlusDescriptionViewModelFactory : ILayoutFactory
{
    public bool IsCompatibleData(Hotspot.Media hotspot)
    {
        var imagesCompatible = hotspot.ImagePaths.Count == 0;
        var videosCompatible = hotspot.VideoPaths.Count == 1;

        return imagesCompatible && videosCompatible;
    }

    public ILayout CreateLayout(IViewModelProvider vmProvider, Hotspot.Media hotspot)
    {
        if (!IsCompatibleData(hotspot)) throw new ArgumentException("Hotspot invalid for layout type.");

        return new VideoPlusDescriptionViewModel(vmProvider, hotspot.Description, hotspot.VideoPaths.First());
    }
}
