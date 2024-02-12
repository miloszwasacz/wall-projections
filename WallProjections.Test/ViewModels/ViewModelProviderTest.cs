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
    public void GetDisplayViewModelTest()
    {
        var displayViewModel = ViewModelProvider.Instance.GetDisplayViewModel();
        Assert.Multiple(() =>
        {
            Assert.That(displayViewModel, Is.InstanceOf<DisplayViewModel>());
            Assert.That(displayViewModel.ImageViewModel, Is.Not.Null);
            Assert.That(displayViewModel.VideoViewModel, Is.Not.Null);
            Assert.That(displayViewModel.HotspotViewModel, Is.Not.Null);
        });
    }

    [Test]
    public void GetVideoViewModelTest()
    {
        var videoViewModel = ViewModelProvider.Instance.GetVideoViewModel();
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
