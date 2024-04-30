using System.Collections.Immutable;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;
using WallProjections.ViewModels.SecondaryScreens;

namespace WallProjections.Test.ViewModels.SecondaryScreens;

[TestFixture]
public class HotspotProjectionViewModelTest
{
    private static readonly Hotspot Hotspot = new(
        1,
        new Coord(2, 3, 4),
        "Title",
        "",
        ImmutableList<string>.Empty,
        ImmutableList<string>.Empty
    );

    [Test]
    public void ConstructorTest()
    {
        var viewModel = new HotspotProjectionViewModel(Hotspot);
        Assert.Multiple(() =>
        {
            Assert.That(viewModel.Id, Is.EqualTo(Hotspot.Id));
            Assert.That(viewModel.X, Is.EqualTo(Hotspot.Position.X - Hotspot.Position.R));
            Assert.That(viewModel.Y, Is.EqualTo(Hotspot.Position.Y - Hotspot.Position.R));
            Assert.That(viewModel.D, Is.EqualTo(Hotspot.Position.R * 2));
            Assert.That(viewModel.State, Is.EqualTo(HotspotState.None));
        });
    }

    [Test]
    public void IsActivatingTest()
    {
        var viewModel = new HotspotProjectionViewModel(Hotspot);
        Assert.That(viewModel.State, Is.Not.EqualTo(HotspotState.Activating));

        viewModel.State = HotspotState.Activating;
        Assert.That(viewModel.State, Is.EqualTo(HotspotState.Activating));
        
    }

    [Test]
    public void IsDeactivatingTest()
    {
        var viewModel = new HotspotProjectionViewModel(Hotspot);
        Assert.That(viewModel.State, Is.Not.EqualTo(HotspotState.Deactivating));

        viewModel.State = HotspotState.Deactivating;
        Assert.That(viewModel.State, Is.EqualTo(HotspotState.Deactivating));
    }

    [Test]
    public void IsActiveTest()
    {
        var viewModel = new HotspotProjectionViewModel(Hotspot);
        Assert.That(viewModel.State, Is.Not.EqualTo(HotspotState.Active));

        viewModel.State = HotspotState.Active;
        Assert.That(viewModel.State, Is.EqualTo(HotspotState.Active));
    }
}
