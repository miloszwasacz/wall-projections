using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.Models.Interfaces;

public interface ILayoutFactory
{
    /// <summary>
    /// Checks is data is compatible with layout.
    /// </summary>
    /// <param name="hotspot"><see cref="Hotspot"/> to check for compatibility.</param>
    /// <returns><i>true</i> if data is compatible with layout, <i>false</i> otherwise.</returns>
    public bool IsCompatibleData(Hotspot.Media hotspot);

    /// <summary>
    /// Creates a layout for the input hotspot if data compatible.
    /// </summary>
    /// <param name="vmProvider"><see cref="IViewModelProvider"/> to use to produce internal view models.</param>
    /// <param name="hotspot"><see cref="Hotspot"/> to generate layout for.</param>
    /// <returns>Layout with input data and using input view models.</returns>
    public ILayout CreateLayout(IViewModelProvider vmProvider, Hotspot.Media hotspot);
}
