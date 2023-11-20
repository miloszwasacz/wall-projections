using WallProjections.Models;
using WallProjections.Test.Mocks.Models;
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
        var contentProvider = new MockContentProvider(Files);
        var displayViewModel = new DisplayViewModel(ViewModelProvider, contentProvider, HotspotId);
        displayViewModel.Activator.Activate();

        Assert.Multiple(() =>
        {
            //TODO Add proper tests for ContentProvider once it is refactored
            Assert.That(contentProvider.FileNumber, Is.EqualTo(HotspotId));

            Assert.That(displayViewModel.VideoViewModel, Is.Not.Null);
            //TODO Add proper tests for the Description once DisplayViewModel is refactored
            Assert.That(displayViewModel.Description, Is.EqualTo(Files[0].Description));

            var videoViewModel = displayViewModel.VideoViewModel as MockVideoViewModel ?? throw MockException;
            Assert.That(videoViewModel.VideoPath, Is.EqualTo(VideoPath));
        });
    }

    [Test]
    public void CreationNoVideoTest()
    {
        var contentProvider = new MockContentProvider(FilesNoVideo);
        var displayViewModel = new DisplayViewModel(ViewModelProvider, contentProvider, HotspotId);

        Assert.Multiple(() =>
        {
            Assert.That(displayViewModel.VideoViewModel, Is.Null);
            //TODO Add proper tests for the Description once DisplayViewModel is refactored
            Assert.That(displayViewModel.Description, Is.EqualTo(FilesNoVideo[0].Description));
        });
    }

    [Test]
    public void ActivationTest()
    {
        var contentProvider = new MockContentProvider(Files);
        var displayViewModel = new DisplayViewModel(ViewModelProvider, contentProvider, HotspotId);

        // Activate the viewmodel
        var disposable = displayViewModel.Activator.Activate();
        var videoViewModel = displayViewModel.VideoViewModel as MockVideoViewModel ?? throw MockException;
        Assert.Multiple(() =>
        {
            Assert.That(videoViewModel.PlayCounter, Is.EqualTo(1));
            Assert.That(videoViewModel.StopCounter, Is.EqualTo(0));
            Assert.That(videoViewModel.DisposeCounter, Is.EqualTo(0));
        });

        // Deactivate the viewmodel
        disposable.Dispose();
        Assert.Multiple(() =>
        {
            Assert.That(videoViewModel.PlayCounter, Is.EqualTo(1));
            Assert.That(videoViewModel.StopCounter, Is.EqualTo(1));
            Assert.That(videoViewModel.DisposeCounter, Is.EqualTo(1));
        });

        // Activate the viewmodel again
        disposable = displayViewModel.Activator.Activate();
        var videoViewModel2 = displayViewModel.VideoViewModel as MockVideoViewModel ?? throw MockException;
        Assert.Multiple(() =>
        {
            Assert.That(videoViewModel.PlayCounter, Is.EqualTo(1));
            Assert.That(videoViewModel.StopCounter, Is.EqualTo(1));
            Assert.That(videoViewModel.DisposeCounter, Is.EqualTo(1));

            Assert.That(videoViewModel2.PlayCounter, Is.EqualTo(1));
            Assert.That(videoViewModel2.StopCounter, Is.EqualTo(0));
            Assert.That(videoViewModel2.DisposeCounter, Is.EqualTo(0));
        });
        disposable.Dispose();
    }

    [Test]
    public void ActivationNoVideoTest()
    {
        var contentProvider = new MockContentProvider(FilesNoVideo);
        var displayViewModel = new DisplayViewModel(ViewModelProvider, contentProvider, HotspotId);

        // Activate the viewmodel
        var disposable = displayViewModel.Activator.Activate();
        Assert.That(displayViewModel.VideoViewModel, Is.Null);

        // Deactivate the viewmodel
        disposable.Dispose();
        Assert.That(displayViewModel.VideoViewModel, Is.Null);

        // Activate the viewmodel again
        disposable = displayViewModel.Activator.Activate();
        Assert.That(displayViewModel.VideoViewModel, Is.Null);
        disposable.Dispose();
    }


    [Test]
    public void ActivationNoMediaPlayerTest()
    {
        var contentProvider = new MockContentProvider(Files);
        var displayViewModel = new DisplayViewModel(ViewModelProvider, contentProvider, HotspotId);
        displayViewModel.Activator.Activate();
        var videoViewModel = displayViewModel.VideoViewModel as MockVideoViewModel ?? throw MockException;
        videoViewModel.CanPlay = false;

        // Activate the viewmodel
        var disposable = displayViewModel.Activator.Activate();
        Assert.Multiple(() =>
        {
            Assert.That(displayViewModel.VideoViewModel, Is.Not.Null);
            Assert.That(videoViewModel.PlayCounter, Is.EqualTo(1));
            Assert.That(videoViewModel.StopCounter, Is.EqualTo(0));
            Assert.That(videoViewModel.DisposeCounter, Is.EqualTo(0));
        });
        disposable.Dispose();
    }
}
