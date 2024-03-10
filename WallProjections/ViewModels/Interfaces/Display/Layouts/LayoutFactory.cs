using System;
using WallProjections.Models;
using WallProjections.ViewModels.Display.Layouts;

namespace WallProjections.ViewModels.Interfaces.Display.Layouts;

/// <summary>
/// A factory for creating layouts for hotspots.
/// </summary>
/// <remarks>
/// <see cref="LayoutProvider" /> looks for this class for automatic detection of available layout types.
/// </remarks>
public abstract class LayoutFactory
{
    /// <summary>
    /// Checks if the hotspot data is compatible with this layout type.
    /// </summary>
    /// <param name="hotspot"><see cref="Hotspot"/> to check for compatibility.</param>
    /// <returns>Whether the data is compatible with the layout type.</returns>
    public abstract bool IsCompatibleData(Hotspot.Media hotspot);

    /// <summary>
    /// Constructs the layout for the input hotspot (usually just a constructor call).
    /// </summary>
    /// <param name="vmProvider">The <see cref="IViewModelProvider" /> to use to produce internal viewmodels.</param>
    /// <param name="hotspot">The <see cref="Hotspot" /> to generate layout for.</param>
    /// <returns>Layout with input data and using input viewmodels.</returns>
    /// <remarks>This method is used in <see cref="CreateLayout" />.</remarks>
    protected abstract Layout ConstructLayout(IViewModelProvider vmProvider, Hotspot.Media hotspot);

    /// <summary>
    /// Creates a layout for the input hotspot if the data is compatible.
    /// </summary>
    /// <param name="vmProvider"><see cref="IViewModelProvider"/> to use to produce internal view models.</param>
    /// <param name="hotspot"><see cref="Hotspot"/> to generate layout for.</param>
    /// <returns>Layout with input data and using input view models.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the input hotspot is not compatible with this layout type.
    /// </exception>
    public Layout CreateLayout(IViewModelProvider vmProvider, Hotspot.Media hotspot)
    {
        if (!IsCompatibleData(hotspot)) throw new ArgumentException("Hotspot invalid for layout type.");

        return ConstructLayout(vmProvider, hotspot);
    }
}
