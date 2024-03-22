using System.Collections.Immutable;
using WallProjections.Models;
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
            Assert.That(viewModel.IsActive, Is.False);
        });
    }

    [Test]
    public void IsActivatingTest()
    {
        var viewModel = new HotspotProjectionViewModel(Hotspot);
        Assert.That(viewModel.IsActivating, Is.False);

        viewModel.IsActivating = true;
        Assert.That(viewModel.IsActivating, Is.True);

        // Changing IsActive or IsDeactivating does not change IsActivating
        viewModel.IsActive = true;
        Assert.That(viewModel.IsActivating, Is.True);
        viewModel.IsDeactivating = true;
        Assert.That(viewModel.IsActivating, Is.True);
    }

    [Test]
    public void IsDeactivatingTest()
    {
        var viewModel = new HotspotProjectionViewModel(Hotspot);
        Assert.That(viewModel.IsDeactivating, Is.False);

        viewModel.IsDeactivating = true;
        Assert.That(viewModel.IsDeactivating, Is.True);

        // Changing IsActive or IsActivating does not change IsDeactivating
        viewModel.IsActive = true;
        Assert.That(viewModel.IsDeactivating, Is.True);
        viewModel.IsActivating = true;
        Assert.That(viewModel.IsDeactivating, Is.True);
    }

    [Test]
    public void IsActiveTest()
    {
        var viewModel = new HotspotProjectionViewModel(Hotspot);
        Assert.That(viewModel.IsActive, Is.False);

        viewModel.IsActive = true;
        Assert.That(viewModel.IsActive, Is.True);

        // Changing IsActivating or IsDeactivating does not change IsActive
        viewModel.IsActivating = true;
        Assert.That(viewModel.IsActive, Is.True);
        viewModel.IsDeactivating = true;
        Assert.That(viewModel.IsActive, Is.True);
    }
}
