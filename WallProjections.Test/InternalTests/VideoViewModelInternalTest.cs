using System.Reflection;
using LibVLCSharp.Shared;
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
        var videoViewModel = new VideoViewModel(_libVlc, mediaPlayer);
        var newMediaPlayer = new MockMediaPlayer();

        var setter = videoViewModel.GetType().GetProperty("MediaPlayer")?.GetSetMethod(true) ?? throw PropertyException;
        Assert.Throws<InvalidOperationException>(() =>
        {
            try
            {
                setter.Invoke(videoViewModel, new object[] { newMediaPlayer });
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException is not null)
                    throw e.InnerException;
                throw;
            }
        });
    }
}
