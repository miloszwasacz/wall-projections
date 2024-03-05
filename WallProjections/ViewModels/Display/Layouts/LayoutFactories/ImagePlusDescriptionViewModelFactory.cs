using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.ViewModels.Display.Layouts.LayoutFactories;

public class ImagePlusDescriptionViewModelFactory : ILayoutFactory
{
    public bool IsCompatibleData(Hotspot.Media hotspot)
    {
        var videosCompatible = hotspot.VideoPaths.Count == 0;
        var imagesCompatible = hotspot.ImagePaths.Count > 0;

        return videosCompatible && imagesCompatible;
    }

    public ILayout CreateLayout(IViewModelProvider vmProvider, Hotspot.Media hotspot)
    {
        return new ImagePlusDescriptionViewModel(
            vmProvider,
            hotspot.Title,
            hotspot.Description,
            hotspot.ImagePaths);
    }
}
