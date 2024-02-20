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
        hotspotViewModel.Coordinates.Add(new HotspotProjection
            { Id = 0, X = 10, Y = 10, D = 10, IsActive = false });
        hotspotViewModel.Coordinates.Add(new HotspotProjection
            { Id = 1, X = 50, Y = 50, D = 14, IsActive = false });
        hotspotViewModel.Coordinates.Add(new HotspotProjection
            { Id = 2, X = 100, Y = 200, D = 20, IsActive = false });
        hotspotViewModel.ActivateHotspot(0);
        Assert.Multiple(() =>
        {
            Assert.That(hotspotViewModel.Coordinates[0].IsActive, Is.True);
            Assert.That(hotspotViewModel.Coordinates[1].IsActive, Is.False);
            Assert.That(hotspotViewModel.Coordinates[2].IsActive, Is.False);
        });
        hotspotViewModel.ActivateHotspot(1);
        Assert.Multiple(() =>
        {
            Assert.That(hotspotViewModel.Coordinates[0].IsActive, Is.False);
            Assert.That(hotspotViewModel.Coordinates[1].IsActive, Is.True);
            Assert.That(hotspotViewModel.Coordinates[2].IsActive, Is.False);
        });
        hotspotViewModel.DeactivateHotspots();
        Assert.Multiple(() =>
        {
            Assert.That(hotspotViewModel.Coordinates[0].IsActive, Is.False);
            Assert.That(hotspotViewModel.Coordinates[1].IsActive, Is.False);
            Assert.That(hotspotViewModel.Coordinates[2].IsActive, Is.False);
        });
    }

    [AvaloniaTest]
    public void DisplayHotspotsTest()
    {
        var hotspotViewModel = new HotspotViewModel();
        Assert.That(hotspotViewModel.IsVisible, Is.False);
        hotspotViewModel.DisplayHotspots();
        Assert.That(hotspotViewModel.IsVisible, Is.True);
    }
}
