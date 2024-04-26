using LibVLCSharp.Shared;
using WallProjections.Test.Mocks;
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
    [Timeout(5000)]
    public async Task CreationTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer, new MockLoggerFactory());
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

        await MarkLoadedAndWait(videoViewModel);
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
    [Timeout(5000)]
    public async Task PlayVideoTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        using IVideoViewModel videoViewModel = new VideoViewModel(LibVlc, mediaPlayer, new MockLoggerFactory());
        await MarkLoadedAndWait(videoViewModel);
        Assert.Multiple(() =>
        {
            Assert.That(videoViewModel.PlayVideo(VideoPath), Is.True);
            Assert.That(mediaPlayer.HasPlayedOnly(VideoPath), Is.True);
            Assert.That(videoViewModel.IsVisible, Is.True);
        });
    }

    [Test]
    [Timeout(5000)]
    public async Task PlayMultipleVideosTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        using var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer, new MockLoggerFactory());
        await MarkLoadedAndWait(videoViewModel);
        var paths = new[] { VideoPath, "test2.mp4" };

        Assert.That(videoViewModel.PlayVideos(paths), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(mediaPlayer.LastPlayedVideo, Does.EndWith(paths[0]));
            Assert.That(videoViewModel.IsVisible, Is.True);
        });

        mediaPlayer.MarkVideoAsEnded();
        Assert.Multiple(() =>
        {
            Assert.That(mediaPlayer.LastPlayedVideo, Does.EndWith(paths[1]));
            Assert.That(videoViewModel.IsVisible, Is.True);
        });

        mediaPlayer.MarkVideoAsEnded();
        Assert.Multiple(() =>
        {
            Assert.That(videoViewModel.HasVideos, Is.False);
            Assert.That(videoViewModel.IsVisible, Is.False);
        });
    }

    [Test]
    [Timeout(5000)]
    public async Task PlayNonExistentVideoTest()
    {
        const string path = "nonexistent.mp4";
        var mediaPlayer = new MockMediaPlayer(false);
        using var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer, new MockLoggerFactory());
        await MarkLoadedAndWait(videoViewModel);
        Assert.Multiple(() =>
        {
            Assert.That(videoViewModel.PlayVideos(new[] { path }), Is.False);
            Assert.That(videoViewModel.IsVisible, Is.False);
        });
    }

    [Test]
    [Timeout(5000)]
    public async Task StopVideoTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        using var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer, new MockLoggerFactory());
        await MarkLoadedAndWait(videoViewModel);
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
    [Timeout(5000)]
    public async Task VolumeTest()
    {
        const int volume = 50;
        var mediaPlayer = new MockMediaPlayer();
        using var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer, new MockLoggerFactory());
        await MarkLoadedAndWait(videoViewModel);
        videoViewModel.PlayVideos(new[] { VideoPath });

        videoViewModel.Volume = volume;
        Assert.Multiple(() =>
        {
            Assert.That(videoViewModel.Volume, Is.EqualTo(volume));
            Assert.That(mediaPlayer.Volume, Is.EqualTo(volume));
        });
    }

    [Test]
    [Timeout(5000)]
    public async Task DisposeTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer, new MockLoggerFactory());
        await MarkLoadedAndWait(videoViewModel);
        videoViewModel.Dispose();
        Assert.That(mediaPlayer.HasBeenDisposedOnce(), Is.True);
    }

    [Test]
    [Timeout(5000)]
    public async Task PlayOrStopVideoAfterDisposeTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer, new MockLoggerFactory());
        await MarkLoadedAndWait(videoViewModel);
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
    [Timeout(5000)]
    public async Task DisposeTwiceTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        var videoViewModel = new VideoViewModel(LibVlc, mediaPlayer, new MockLoggerFactory());
        await MarkLoadedAndWait(videoViewModel);
        videoViewModel.Dispose();
        videoViewModel.Dispose();
        Assert.That(mediaPlayer.HasBeenDisposedOnce(), Is.True);
    }

    /// <summary>
    /// Calls <see cref="IVideoViewModel.MarkLoaded" /> and waits for 2 seconds
    /// </summary>
    /// <param name="videoViewModel">The viewmodel to mark as loaded</param>
    private static async Task MarkLoadedAndWait(IVideoViewModel videoViewModel)
    {
        videoViewModel.MarkLoaded();
        await Task.Delay(2000);
    }
}
