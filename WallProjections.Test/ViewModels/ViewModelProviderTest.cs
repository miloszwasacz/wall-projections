using WallProjections.Test.Mocks.Models;
using WallProjections.ViewModels;

namespace WallProjections.Test.ViewModels;

[TestFixture]
[NonParallelizable]
public class ViewModelProviderTest
{
    [Test]
    public void SingletonPatternTest()
    {
        var instance1 = ViewModelProvider.Instance;
        var instance2 = ViewModelProvider.Instance;

        Assert.That(instance2, Is.SameAs(instance1));
    }

    [Test]
    public void GetMainWindowViewModelTest()
    {
        var mainWindowViewModel = ViewModelProvider.Instance.GetMainWindowViewModel();
        Assert.That(mainWindowViewModel, Is.InstanceOf<MainWindowViewModel>());
        Assert.That(mainWindowViewModel.DisplayViewModel, Is.Null);
    }

    [Test]
    public void GetDisplayViewModelTest()
    {
        const string artifactId = "test";
        var files = new[] { "test", "test.mp4" };
        var fileProvider = new MockFileProvider(files);
        var displayViewModel = ViewModelProvider.Instance.GetDisplayViewModel(artifactId, fileProvider);
        Assert.That(displayViewModel, Is.InstanceOf<DisplayViewModel>());
        Assert.That(displayViewModel.VideoViewModel, Is.Not.Null);
    }

    [Test]
    public void GetVideoViewModelTest()
    {
        const string videoPath = "test.mp4";
        var videoViewModel = ViewModelProvider.Instance.GetVideoViewModel(videoPath);
        Assert.That(videoViewModel, Is.InstanceOf<VideoViewModel>());
        Assert.That(videoViewModel.MediaPlayer, Is.Not.Null);
    }

    [Test]
    [NonParallelizable]
    public void UsingAfterDisposingTest()
    {
        ViewModelProvider.Instance.Dispose();
        Assert.That(ViewModelProvider.Instance, Is.Not.Null);
        GetVideoViewModelTest();
    }
}
