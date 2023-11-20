using Avalonia.Headless.NUnit;
using WallProjections.Models;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;

namespace WallProjections.Test.Views;

[TestFixture]
public class MainWindowTest
{
    private const int HotspotId = 1;

    private static readonly Hotspot.Media Files = new(
        new Hotspot(HotspotId),
        HotspotId + " description",
        HotspotId + ".png",
        HotspotId + ".mp4"
    );

    private static readonly MockViewModelProvider VmProvider = new();
    private static readonly MockContentProvider ContentProvider = new(new List<Hotspot.Media> { Files });

    [AvaloniaTest]
    public void ViewModelTest()
    {
        var vm = VmProvider.GetMainWindowViewModel();
        var mainWindow = new WallProjections.Views.MainWindow
        {
            DataContext = vm
        };

        Assert.Multiple(() =>
        {
            Assert.That(mainWindow.DataContext, Is.SameAs(vm));
            Assert.That(mainWindow.DisplayView.DataContext, Is.Null);
        });
    }

    //TODO Rename this test once MainWindow has been refactored
    [AvaloniaTest]
    public void CreateDisplayViewModelTest()
    {
        var vm = VmProvider.GetMainWindowViewModel();
        var mainWindow = new WallProjections.Views.MainWindow
        {
            DataContext = vm
        };
        mainWindow.Show();
        vm.CreateDisplayViewModel(HotspotId, ContentProvider);


        Assert.Multiple(() =>
        {
            Assert.That(mainWindow.DisplayView.DataContext, Is.Not.Null);
            Assert.That(mainWindow.DisplayView.DataContext, Is.SameAs(vm.DisplayViewModel));
        });
    }

    //TODO Add more proper tests once MainWindow has been refactored
}
