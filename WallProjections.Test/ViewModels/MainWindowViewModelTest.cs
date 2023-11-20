using WallProjections.Models;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.ViewModels;

namespace WallProjections.Test.ViewModels;

[TestFixture]
public class MainWindowViewModelTest
{
    private static readonly int[] HotspotIds = { 1, 2 };

    [Test]
    public void CreationTest()
    {
        var viewModelProvider = new MockViewModelProvider();
        var mainWindowViewModel = new MainWindowViewModel(viewModelProvider);

        Assert.That(mainWindowViewModel.DisplayViewModel, Is.Null);
    }

    [Test]
    public void CreateDisplayViewModelTest()
    {
        var viewModelProvider = new MockViewModelProvider();

        var media = HotspotIds.Select(id => new Hotspot.Media(
            new Hotspot(id),
            id + " description",
            id + ".png",
            id + ".mp4"
        )).ToList();
        var contentProvider = new MockContentProvider(media);
        var mainWindowViewModel = new MainWindowViewModel(viewModelProvider);

        // Create a new DisplayViewModel
        mainWindowViewModel.CreateDisplayViewModel(HotspotIds[0], contentProvider);
        Assert.That(mainWindowViewModel.DisplayViewModel, Is.Not.Null);
        Assert.That(mainWindowViewModel.DisplayViewModel?.Description,
            Is.EqualTo(MockDisplayViewModel.DefaultDescription + HotspotIds[0]));

        // Change the DisplayViewModel
        mainWindowViewModel.CreateDisplayViewModel(HotspotIds[1], contentProvider);
        Assert.That(mainWindowViewModel.DisplayViewModel, Is.Not.Null);
        Assert.That(mainWindowViewModel.DisplayViewModel?.Description,
            Is.EqualTo(MockDisplayViewModel.DefaultDescription + HotspotIds[1]));
    }
}
