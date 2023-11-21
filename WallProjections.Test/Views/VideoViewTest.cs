using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.ViewModels;
using WallProjections.Views;

namespace WallProjections.Test.Views;

[TestFixture]
public class VideoViewTest
{
    [AvaloniaTest]
    public void ViewModelTest()
    {
        var videoViewModel = ViewModelProvider.Instance.GetVideoViewModel();
        var displayViewModel = new MockDisplayViewModel(videoViewModel);
        var displayWindow = new DisplayWindow
        {
            DataContext = displayViewModel
        };
        var vm = displayViewModel.VideoViewModel;
        var videoView = displayWindow.VideoView;

        Assert.That(videoView.MediaPlayer, Is.Not.Null);
        Assert.That(videoView.MediaPlayer, Is.SameAs(vm.MediaPlayer));
        displayViewModel.Dispose();
    }

    [AvaloniaTest]
    public void ResizeWidthTest()
    {
        var initSize = new Size(1920, 1080);
        var newSize = new Size(1280, initSize.Height);

        // Initialize the view and window
        var window = new DisplayWindow
        {
            Width = initSize.Width,
            Height = initSize.Height,
        };
        var videoView = window.VideoView;
        window.Show();

        // Check that the view and window are initialized correctly
        Assert.That(videoView.Bounds.Height, Is.EqualTo(videoView.Bounds.Width * 9 / 16));

        // Resize the view
        var args = new SizeChangedEventArgs(null, null, initSize, newSize);
        window.OnVideoViewResize(videoView, args);

        // Check that the ratio is preserved
        Assert.That(videoView.Bounds.Height, Is.EqualTo(videoView.Bounds.Width * 9 / 16));
    }

    [AvaloniaTest]
    public void ResizeHeightTest()
    {
        var initSize = new Size(1920, 1080);
        var newSize = new Size(initSize.Width, 720);

        // Initialize the view and window
        var window = new DisplayWindow
        {
            Width = initSize.Width,
            Height = initSize.Height,
        };
        var videoView = window.VideoView;
        window.Show();

        // Check that the view and window are initialized correctly
        Assert.That(videoView.Bounds.Height, Is.EqualTo(videoView.Bounds.Width * 9 / 16));

        // Resize the view
        var args = new SizeChangedEventArgs(null, null, initSize, newSize);
        window.OnVideoViewResize(videoView, args);

        // Check that the ratio is preserved
        Assert.That(videoView.Bounds.Height, Is.EqualTo(videoView.Bounds.Width * 9 / 16));
    }
}
