using System.Reflection;
using LibVLCSharp.Shared;
using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks;
using WallProjections.Test.Mocks.Models;
using WallProjections.ViewModels.Display;

namespace WallProjections.Test.InternalTests;

[TestFixture]
public class VideoViewModelInternalTest
{
    private static AssertionException PropertyException =>
        new("VideoViewModel does not have a setter for the property");

    private LibVLC _libVlc = null!;

    [SetUp]
    public void SetUp()
    {
        _libVlc = new LibVLC();
    }

    [TearDown]
    public void TearDown()
    {
        _libVlc.Dispose();
    }

    [Test]
    public void MediaPlayerSetterTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        var videoViewModel = new VideoViewModel(_libVlc, mediaPlayer, new MockLoggerFactory());
        videoViewModel.MarkLoaded();

        var newMediaPlayer = new MockMediaPlayer();
        Assert.Throws<InvalidOperationException>(() =>
        {
            try
            {
                SetMediaPlayer(videoViewModel, newMediaPlayer);
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException is not null)
                    throw e.InnerException;
                throw;
            }
        });
    }

    [Test]
    public void DisposeNoMediaPlayerTest()
    {
        var videoViewModel = new VideoViewModel(_libVlc, new MockMediaPlayer(), new MockLoggerFactory());
        videoViewModel.MarkLoaded();
        SetMediaPlayer(videoViewModel, null);

        videoViewModel.Dispose();
        Assert.That(videoViewModel.MediaPlayer, Is.Null);
    }

    /// <summary>
    /// Uses reflection to use the private setter of <see cref="VideoViewModel.MediaPlayer"/>
    /// </summary>
    private static void SetMediaPlayer(VideoViewModel videoViewModel, IMediaPlayer? mediaPlayer)
    {
        var setter = videoViewModel.GetType().GetProperty("MediaPlayer")?.GetSetMethod(true) ?? throw PropertyException;
        setter.Invoke(videoViewModel, new object?[] { mediaPlayer });
    }
}
