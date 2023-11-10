using WallProjections.Test.Mocks;
using WallProjections.ViewModels;

namespace WallProjections.Test.ViewModels;

[TestFixture]
public class VideoViewModelTest
{
    private const string VideoPath = "test.mp4";

    [Test]
    public void CreationTest()
    {
        var videoViewModel = new VideoViewModel(VideoPath);
        Assert.That(videoViewModel.MediaPlayer, Is.Not.Null);
        videoViewModel.Dispose();
        Assert.That(videoViewModel.MediaPlayer, Is.Null);
    }

    [Test]
    public void PlayVideoTest()
    {
        var videoViewModel = new VideoViewModel(VideoPath);
        var mediaPlayer = MediaPlayerMock.InjectInto(videoViewModel);
        videoViewModel.PlayVideo();
        Assert.That(mediaPlayer.HasPlayedOnly(VideoPath));
        videoViewModel.Dispose();
    }

    [Test]
    public void StopVideoTest()
    {
        var videoViewModel = new VideoViewModel(VideoPath);
        var mediaPlayer = MediaPlayerMock.InjectInto(videoViewModel);
        videoViewModel.PlayVideo();
        videoViewModel.StopVideo();
        Assert.Multiple(() =>
        {
            Assert.That(mediaPlayer.HasStopped());
            Assert.That(mediaPlayer.HasNotBeenDisposed());
        });
        videoViewModel.Dispose();
    }

    [Test]
    public void DisposeTest()
    {
        var videoViewModel = new VideoViewModel(VideoPath);
        var mediaPlayer = MediaPlayerMock.InjectInto(videoViewModel);
        videoViewModel.Dispose();
        Assert.That(mediaPlayer.HasBeenDisposedOnce());
    }

    [Test]
    public void StopVideoAfterDisposeTest()
    {
        var videoViewModel = new VideoViewModel(VideoPath);
        var mediaPlayer = MediaPlayerMock.InjectInto(videoViewModel);
        videoViewModel.PlayVideo();
        videoViewModel.Dispose();
        Assert.Multiple(() =>
        {
            Assert.That(mediaPlayer.HasStopped());
            Assert.That(mediaPlayer.HasBeenDisposedOnce());
        });
    }

    [Test]
    public void DisposeTwiceTest()
    {
        var videoViewModel = new VideoViewModel(VideoPath);
        var mediaPlayer = MediaPlayerMock.InjectInto(videoViewModel);
        videoViewModel.Dispose();
        videoViewModel.Dispose();
        Assert.That(mediaPlayer.HasBeenDisposedOnce());
    }
}
