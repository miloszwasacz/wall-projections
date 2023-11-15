using System.Reflection;
using LibVLCSharp.Shared;
using WallProjections.Test.Mocks.Models;
using WallProjections.ViewModels;

namespace WallProjections.Test.InternalTests;

[TestFixture]
public class VideoViewModelInternalTest
{
    private static AssertionException PropertyException =>
        new("VideoViewModel does not have a setter for the property");

    [Test]
    public void MediaPlayerSetterTest()
    {
        var mediaPlayer = new MockMediaPlayer();
        var videoViewModel = new VideoViewModel("test.mp4", new LibVLC(), mediaPlayer);
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
                throw e.InnerException ?? throw e;
            }
        });
    }
}
