using LibVLCSharp.Shared;
using WallProjections.Test.Mocks.Models;
using WallProjections.ViewModels;

namespace WallProjections.Test.ViewModels;

[TestFixture]
public class VideoViewModelTest
{
    private const string VideoPath = "test.mp4";
    private static LibVLC LibVlc => new();

    [Test]
    public void CreationTest()
    {
        var videoViewModel = new VideoViewModel(LibVlc, new MockMediaPlayer());
        Assert.That(videoViewModel.MediaPlayer, Is.Not.Null);
        videoViewModel.Dispose();
        Assert.That(videoViewModel.MediaPlayer, Is.Null);
    }

    [Test]
    public void PlayVideoTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer);
        Assert.Multiple(() =>
        {
            Assert.That(videoViewModel.PlayVideo(VideoPath), Is.True);
            Assert.That(mediaPlayer.HasPlayedOnly(VideoPath), Is.True);
        });
        videoViewModel.Dispose();
    }

    [Test]
    public void PlayNonExistentVideoTest()
    {
        var path = "nonexistent.mp4";
        var mediaPlayer = new MockMediaPlayer(false);
        var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer);
        Assert.That(videoViewModel.PlayVideo(path), Is.False);
        videoViewModel.Dispose();
    }

    [Test]
    public void StopVideoTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer);
        videoViewModel.PlayVideo(VideoPath);
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
        var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer);
        videoViewModel.Dispose();
        Assert.That(mediaPlayer.HasBeenDisposedOnce(), Is.True);
    }

    [Test]
    public void StopVideoAfterDisposeTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer);
        videoViewModel.PlayVideo(VideoPath);
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
        var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer);
        videoViewModel.Dispose();
        videoViewModel.Dispose();
        Assert.That(mediaPlayer.HasBeenDisposedOnce(), Is.True);
    }
}
