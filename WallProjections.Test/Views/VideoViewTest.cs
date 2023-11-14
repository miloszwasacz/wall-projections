using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using WallProjections.ViewModels;

namespace WallProjections.Test.Views;

[TestFixture]
public class VideoViewTest
{
    private const string VideoPath = "test.mp4";

    [AvaloniaTest]
    public void ViewModelTest()
    {
        var vm = ViewModelProvider.Instance.GetVideoViewModel(VideoPath);
        var videoView = new WallProjections.Views.VideoView
        {
            DataContext = vm
        };

        Assert.That(videoView.VideoPlayer.MediaPlayer, Is.Not.Null);
        Assert.That(videoView.VideoPlayer.MediaPlayer, Is.SameAs(vm.MediaPlayer));
    }

    [AvaloniaTest]
    public void ResizeTest()
    {
        var initSize = new Size(1920, 1080);
        var newSize = new Size(1280, initSize.Height);
        const int expectedHeight = 720;

        // Initialize the view and window
        var videoView = new WallProjections.Views.VideoView
        {
            Width = initSize.Width,
            Height = initSize.Height,
        };
        var window = new Window
        {
            Content = videoView,
            Width = initSize.Width,
            Height = initSize.Height,
        };
        window.Show();

        // Check that the view and window are initialized correctly
        Assert.Multiple(() =>
        {
            Assert.That(videoView.Bounds.Width, Is.EqualTo(initSize.Width));
            Assert.That(videoView.Bounds.Height, Is.EqualTo(initSize.Height));
        });

        // Resize the view
        var args = new SizeChangedEventArgs(null, null, initSize, newSize);
        videoView.OnResize(videoView, args);

        // Check that the view has corrected its height
        Assert.That(videoView.Height, Is.EqualTo(expectedHeight));
    }
}
