using Avalonia.Headless.NUnit;
using WallProjections.Models;
using WallProjections.ViewModels;

namespace WallProjections.Test.ViewModels;

[TestFixture]
public class HotspotViewModelTest
{
    [AvaloniaTest]
    public void ActivateDeactivateHotspotsTest()
    {
        var hotspotViewModel = new HotspotViewModel();
        hotspotViewModel.Coordinates.Add(new HotCoord(0, 10, 10, 5, 10, false));
        hotspotViewModel.Coordinates.Add(new HotCoord(1, 50, 50, 7, 14, false));
        hotspotViewModel.Coordinates.Add(new HotCoord(2, 100, 200, 10, 20, false));
        hotspotViewModel.ActivateHotspot(0);
        Assert.Multiple(() =>
        {
            Assert.That(hotspotViewModel.Coordinates[0].Vis, Is.True);
            Assert.That(hotspotViewModel.Coordinates[1].Vis, Is.False);
            Assert.That(hotspotViewModel.Coordinates[2].Vis, Is.False);
        });
        hotspotViewModel.ActivateHotspot(1);
        Assert.Multiple(() =>
        {
            Assert.That(hotspotViewModel.Coordinates[0].Vis, Is.False);
            Assert.That(hotspotViewModel.Coordinates[1].Vis, Is.True);
            Assert.That(hotspotViewModel.Coordinates[2].Vis, Is.False);
        });
        hotspotViewModel.DeactivateHotspots();
        Assert.Multiple(() =>
        {
            Assert.That(hotspotViewModel.Coordinates[0].Vis, Is.False);
            Assert.That(hotspotViewModel.Coordinates[1].Vis, Is.False);
            Assert.That(hotspotViewModel.Coordinates[2].Vis, Is.False);
        });
    }

    [AvaloniaTest]
    public void DisplayHotspotsTest()
    {
        var hotspotViewModel = new HotspotViewModel();
        Assert.That(hotspotViewModel.ShowHotspots, Is.False);
        hotspotViewModel.DisplayHotspots();
        Assert.That(hotspotViewModel.ShowHotspots, Is.True);
    }
}
