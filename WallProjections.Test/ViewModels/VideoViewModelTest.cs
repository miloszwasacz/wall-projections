using LibVLCSharp.Shared;
using WallProjections.Test.Mocks;
using WallProjections.ViewModels;

namespace WallProjections.Test.ViewModels;

[TestFixture]
public class VideoViewModelTest
{
    private const string VideoPath = "test.mp4";
    private static readonly LibVLC LibVlc = new();

    [Test]
    public void CreationTest()
    {
        var videoViewModel = new VideoViewModel(VideoPath, LibVlc, new MockMediaPlayer());
        Assert.That(videoViewModel.MediaPlayer, Is.Not.Null);
        videoViewModel.Dispose();
        Assert.That(videoViewModel.MediaPlayer, Is.Null);
    }

    [Test]
    public void PlayVideoTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        var videoViewModel = new VideoViewModel(VideoPath, LibVlc, mediaPlayer);
        Assert.Multiple(() =>
        {
            Assert.That(videoViewModel.PlayVideo(), Is.True);
            Assert.That(mediaPlayer.HasPlayedOnly(VideoPath), Is.True);
        });
        videoViewModel.Dispose();
    }

    [Test]
    public void PlayNonExistentVideoTest()
    {
        var mediaPlayer = new MockMediaPlayer(false);
        var videoViewModel = new VideoViewModel("nonexistent.mp4", LibVlc, mediaPlayer);
        Assert.That(videoViewModel.PlayVideo(), Is.False);
        videoViewModel.Dispose();
    }

    [Test]
    public void StopVideoTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        var videoViewModel = new VideoViewModel(VideoPath, LibVlc, mediaPlayer);
        videoViewModel.PlayVideo();
        videoViewModel.StopVideo();
        Assert.Multiple(() =>
        {
            Assert.That(mediaPlayer.HasStopped(), Is.True);
            Assert.That(mediaPlayer.HasNotBeenDisposed(), Is.True);
        });
        videoViewModel.Dispose();
    }

    [Test]
    public void DisposeTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        var videoViewModel = new VideoViewModel(VideoPath, LibVlc, mediaPlayer);
        videoViewModel.Dispose();
        Assert.That(mediaPlayer.HasBeenDisposedOnce(), Is.True);
    }

    [Test]
    public void StopVideoAfterDisposeTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        var videoViewModel = new VideoViewModel(VideoPath, LibVlc, mediaPlayer);
        videoViewModel.PlayVideo();
        videoViewModel.Dispose();
        Assert.Multiple(() =>
        {
            Assert.That(mediaPlayer.HasStopped(), Is.True);
            Assert.That(mediaPlayer.HasBeenDisposedOnce(), Is.True);
        });
    }

    [Test]
    public void DisposeTwiceTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        var videoViewModel = new VideoViewModel(VideoPath, LibVlc, mediaPlayer);
        videoViewModel.Dispose();
        videoViewModel.Dispose();
        Assert.That(mediaPlayer.HasBeenDisposedOnce(), Is.True);
    }
}
