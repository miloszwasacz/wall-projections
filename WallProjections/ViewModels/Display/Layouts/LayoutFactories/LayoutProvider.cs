using System.Collections.Generic;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Display.Layouts;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.Models.LayoutFactories;

public class LayoutProvider : ILayoutProvider
{
    private readonly IReadOnlyCollection<ILayoutFactory> _layoutFactories
        = new List<ILayoutFactory>
        {
            new DescriptionViewModelFactory(),
            new VideoPlusDescriptionViewModelFactory()
        };

    public ILayout GetLayout(IViewModelProvider vmProvider, Hotspot.Media hotspot)
    {
        foreach (var layoutFactory in _layoutFactories)
        {
            if (layoutFactory.IsCompatibleData(hotspot))
            {
                return layoutFactory.CreateLayout(vmProvider, hotspot);
            }
        }

        return new DescriptionViewModel("Cannot show the information for this hotspot." +
                                        "\nPlease ask a member of staff for help.");
    }
}
