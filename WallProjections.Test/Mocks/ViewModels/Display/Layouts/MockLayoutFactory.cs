using WallProjections.Models;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.Test.Mocks.ViewModels.Display.Layouts;

public class MockLayoutFactory : LayoutFactory
{
    /// <summary>
    /// The output of <see cref="IsCompatibleData" /> (<i>true</i> by default).
    /// </summary>
    public bool IsCompatible { get; init; } = true;

    /// <returns><see cref="IsCompatible" /></returns>
    public override bool IsCompatibleData(Hotspot.Media hotspot) => IsCompatible;

    /// <returns><see cref="MockGenericLayout" /> with the given <paramref name="hotspot" /></returns>
    protected override Layout ConstructLayout(IViewModelProvider vmProvider, Hotspot.Media hotspot) =>
        new MockGenericLayout(hotspot);
}
