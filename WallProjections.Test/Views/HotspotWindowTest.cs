using WallProjections.Test.Mocks.ViewModels;
using HotspotDisplayWindow = WallProjections.Views.Display.HotspotDisplayWindow;

namespace WallProjections.Test.Views;

[TestFixture]
public class HotspotWindowTest
{
    [AvaloniaTest]
    public void AllHotspotsDisplayedTest()
    {
        var vm = new MockHotspotViewModel();
        var hotspotWindow = new HotspotDisplayWindow
        {
            DataContext = vm
        };

        Assert.That(hotspotWindow.HotspotList.Items, Has.Count.EqualTo(vm.Projections.Count));
    }
}
