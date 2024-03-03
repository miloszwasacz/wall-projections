using Avalonia.Threading;
using Avalonia.VisualTree;
using WallProjections.Models;
using WallProjections.Test.Mocks.Helper;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.ViewModels.SecondaryScreens;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;
using WallProjections.Views;
using WallProjections.Views.SecondaryScreens;

namespace WallProjections.Test.Views;

[TestFixture]
public class HotspotWindowTest
{
    [AvaloniaTest]
    public void AllHotspotsDisplayedTest()
    {
        // The config is ignored by the MockHotspotDisplayViewModel so we can pass anything here.
        var config = new Config(MockPythonProxy.CalibrationResult, Array.Empty<Hotspot>());

        var vmProvider = new MockViewModelProvider();
        var vm = new MockSecondaryWindowViewModel(vmProvider);
        var secondaryWindow = new SecondaryWindow
        {
            DataContext = vm
        };
        secondaryWindow.Show();
        vm.ShowHotspotDisplay(config);
        Dispatcher.UIThread.RunJobs();

        var hotspotWindow = secondaryWindow.FindDescendantOfType<HotspotDisplayView>();
        Assert.That(hotspotWindow, Is.Not.Null);
        Assert.That(hotspotWindow!.DataContext, Is.InstanceOf<IHotspotDisplayViewModel>());
        var hotspotViewModel = (IHotspotDisplayViewModel)hotspotWindow.DataContext!;
        Assert.That(hotspotWindow.HotspotList.ItemsSource, Is.SameAs(hotspotViewModel.Projections));
    }
}
