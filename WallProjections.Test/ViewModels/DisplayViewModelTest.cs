using WallProjections.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.ViewModels;

namespace WallProjections.Test.ViewModels;

//TODO Improve tests once DisplayViewModel is refactored
[TestFixture]
public class DisplayViewModelTest
{
    private const int HotspotId = 1;
    private const string Text = "test";
    private const string VideoPath = "test.mp4";

    private static List<Hotspot.Media> Files =>
        new() { new Hotspot.Media(new Hotspot(HotspotId), Text, VideoPath: VideoPath) };

    private static List<Hotspot.Media> FilesNoVideo => new() { new Hotspot.Media(new Hotspot(HotspotId), Text) };
    private static MockViewModelProvider ViewModelProvider => new();
    private static AssertionException MockException => new("VideoViewModel is not a MockVideoViewModel");

    [Test]
    public void CreationTest()
    {
        var displayViewModel = new DisplayViewModel(ViewModelProvider);

        Assert.Multiple(() =>
        {
            //TODO Add proper tests for ContentProvider once it is refactored
            // Use reflection to get the private _contentProvider field

            Assert.That(displayViewModel.ImageViewModel, Is.Not.Null);
            Assert.That(displayViewModel.VideoViewModel, Is.Not.Null);
            //TODO Add proper tests for the Description once DisplayViewModel is refactored
            Assert.That(displayViewModel.Description, Is.Empty);

            var videoViewModel = displayViewModel.VideoViewModel as MockVideoViewModel ?? throw MockException;
            Assert.That(videoViewModel.VideoPaths, Is.Empty);
        });
    }

    [Test]
    public void CreationNoVideoTest()
    {
        var displayViewModel = new DisplayViewModel(ViewModelProvider);

        Assert.Multiple(() =>
        {
            Assert.That(displayViewModel.ImageViewModel, Is.Not.Null);
            Assert.That(displayViewModel.VideoViewModel, Is.Not.Null);
            //TODO Add proper tests for the Description once DisplayViewModel is refactored
            Assert.That(displayViewModel.Description, Is.Empty);
        });
    }
}
