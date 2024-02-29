using Avalonia;
using Avalonia.Controls;
using WallProjections.Test.Mocks.Helper;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.ViewModels.Display;
using WallProjections.ViewModels;
using WallProjections.Views;

namespace WallProjections.Test.Views;

[TestFixture]
public class NativeVideoViewTest
{
    // ReSharper disable once InconsistentNaming
    private readonly ViewModelProvider VMProvider = new(new MockNavigator(), new MockPythonHandler());

    [OneTimeTearDown]
    public void TearDown()
    {
        VMProvider.Dispose();
    }

    [AvaloniaTest]
    public void ViewModelTest()
    {
        var videoViewModel = VMProvider.GetVideoViewModel();
        var displayViewModel = new MockDisplayViewModel(videoViewModel: videoViewModel);
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
    [NonParallelizable]
    public void ResizeWidthTest()
    {
        var initSize = new Size(1920, 1080);
        var newSize = new Size(1280, initSize.Height);

        // Initialize the view and window
        var window = new DisplayWindow
        {
            Width = initSize.Width,
            Height = initSize.Height
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
    [NonParallelizable]
    public void ResizeHeightTest()
    {
        var initSize = new Size(1920, 1080);
        var newSize = new Size(initSize.Width, 720);

        // Initialize the view and window
        var window = new DisplayWindow
        {
            Width = initSize.Width,
            Height = initSize.Height
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

    //TODO Add tests for visibility
}
