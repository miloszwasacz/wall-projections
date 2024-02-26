using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.Models.Interfaces;

public interface ILayoutProvider
{
    /// <summary>
    /// Gets a compatible layout for the hotspot or a layout with an error message.
    /// </summary>
    /// <param name="vmProvider"><see cref="IViewModelProvider"/> to use to produce the internal view models.</param>
    /// <param name="hotspot"><see cref="Hotspot.Media"/> to create layout with.</param>
    /// <returns></returns>
    public ILayout GetLayout(IViewModelProvider vmProvider, Hotspot.Media hotspot);
}
