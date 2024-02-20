using Avalonia.Headless.NUnit;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Views;

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
        
        Assert.That(hotspotWindow.HotspotList.Items.Count, Is.EqualTo(vm.Coordinates.Count));
    }
    
}
