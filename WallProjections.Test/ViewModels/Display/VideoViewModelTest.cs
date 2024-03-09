using LibVLCSharp.Shared;
using WallProjections.Test.Mocks.Models;
using WallProjections.ViewModels.Display;
using WallProjections.ViewModels.Interfaces.Display;

namespace WallProjections.Test.ViewModels.Display;

[TestFixture]
public class VideoViewModelTest
{
    private const string VideoPath = "test.mp4";
    private static LibVLC LibVlc => new();

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        LibVlc.Dispose();
    }

    [Test]
    public void CreationTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer);
        Assert.Multiple(() =>
        {
            Assert.That(videoViewModel.MediaPlayer, Is.Not.Null);
            Assert.That(videoViewModel.HasVideos, Is.False);
            Assert.That(videoViewModel.IsVisible, Is.False);
        });

        Assert.Multiple(() =>
        {
            var success = videoViewModel.PlayVideos(new[] { VideoPath });
            Assert.That(success, Is.False);
            Assert.That(videoViewModel.IsVisible, Is.False);
        });

        videoViewModel.MarkLoaded();
        Assert.Multiple(() =>
        {
            Assert.That(mediaPlayer.IsPlaying);
            Assert.That(videoViewModel.IsVisible, Is.True);
        });

        videoViewModel.Dispose();
        Assert.Multiple(() =>
        {
            Assert.That(videoViewModel.MediaPlayer, Is.Null);
            Assert.That(videoViewModel.IsVisible, Is.False);
        });
    }

    [Test]
    public void PlayVideoTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        using IVideoViewModel videoViewModel = new VideoViewModel(LibVlc, mediaPlayer);
        videoViewModel.MarkLoaded();
        Assert.Multiple(() =>
        {
            Assert.That(videoViewModel.PlayVideo(VideoPath), Is.True);
            Assert.That(mediaPlayer.HasPlayedOnly(VideoPath), Is.True);
            Assert.That(videoViewModel.IsVisible, Is.True);
        });
    }

    [Test]
    public async Task PlayMultipleVideosTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        using var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer);
        videoViewModel.MarkLoaded();
        var paths = new[] { VideoPath, "test2.mp4" };
        Assert.That(videoViewModel.PlayVideos(paths), Is.True);
        await Task.Delay(150);

        var lastPlayedVideo = mediaPlayer.LastPlayedVideo;
        Assert.That(lastPlayedVideo, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(lastPlayedVideo!, Does.EndWith(paths[1]));
            Assert.That(videoViewModel.IsVisible, Is.True);
        });

        await Task.Delay(250);
        Assert.Multiple(() =>
        {
            Assert.That(videoViewModel.HasVideos, Is.False);
            Assert.That(videoViewModel.IsVisible, Is.False);
        });
    }

    [Test]
    public void PlayNonExistentVideoTest()
    {
        const string path = "nonexistent.mp4";
        var mediaPlayer = new MockMediaPlayer(false);
        using var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer);
        videoViewModel.MarkLoaded();
        Assert.Multiple(() =>
        {
            Assert.That(videoViewModel.PlayVideos(new[] { path }), Is.False);
            Assert.That(videoViewModel.IsVisible, Is.False);
        });
    }

    [Test]
    public void StopVideoTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        using var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer);
        videoViewModel.MarkLoaded();
        videoViewModel.PlayVideos(new[] { VideoPath });
        videoViewModel.StopVideo();
        Assert.Multiple(() =>
        {
            Assert.That(mediaPlayer.HasStopped(), Is.True);
            Assert.That(mediaPlayer.HasNotBeenDisposed(), Is.True);
            Assert.That(videoViewModel.IsVisible, Is.False);
        });
    }

    [Test]
    public void VolumeTest()
    {
        const int volume = 50;
        var mediaPlayer = new MockMediaPlayer();
        using var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer);
        videoViewModel.MarkLoaded();
        videoViewModel.PlayVideos(new[] { VideoPath });

        videoViewModel.Volume = volume;
        Assert.Multiple(() =>
        {
            Assert.That(videoViewModel.Volume, Is.EqualTo(volume));
            Assert.That(mediaPlayer.Volume, Is.EqualTo(volume));
        });
    }

    [Test]
    public void DisposeTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer);
        videoViewModel.MarkLoaded();
        videoViewModel.Dispose();
        Assert.That(mediaPlayer.HasBeenDisposedOnce(), Is.True);
    }

    [Test]
    public void PlayOrStopVideoAfterDisposeTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer);
        videoViewModel.MarkLoaded();
        videoViewModel.PlayVideos(new[] { VideoPath });
        videoViewModel.Dispose();
        videoViewModel.PlayVideos(new[] { VideoPath });
        videoViewModel.StopVideo();
        Assert.Multiple(() =>
        {
            Assert.That(mediaPlayer.HasStopped(), Is.True);
            Assert.That(mediaPlayer.HasBeenDisposedOnce(), Is.True);
            Assert.That(videoViewModel.IsVisible, Is.False);
        });
    }

    [Test]
    public void DisposeTwiceTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer);
        videoViewModel.MarkLoaded();
        videoViewModel.Dispose();
        videoViewModel.Dispose();
        Assert.That(mediaPlayer.HasBeenDisposedOnce(), Is.True);
    }
}
