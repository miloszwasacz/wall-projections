using Avalonia.Headless.NUnit;
using WallProjections.Models;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Views;

namespace WallProjections.Test.Views;

[TestFixture]
public class DisplayViewTest
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
        var vm = VmProvider.GetDisplayViewModel(HotspotId, ContentProvider);
        var displayView = new DisplayWindow
        {
            DataContext = vm
        };

        Assert.That(displayView.Description.Text, Is.EqualTo(vm.Description));
        Assert.That(displayView.VideoView.DataContext, Is.Not.Null);
        Assert.That(displayView.VideoView.DataContext, Is.SameAs(vm.VideoViewModel));
    }
}
