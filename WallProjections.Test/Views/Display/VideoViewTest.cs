using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.ViewModels.Display;
using WallProjections.Views.Display;
using static WallProjections.Test.TestExtensions;

namespace WallProjections.Test.Views.Display;

[TestFixture]
public class VideoViewTest
{
    // ReSharper disable once InconsistentNaming
    private readonly MockViewModelProvider VMProvider = new();

    private static IEnumerable<TestCaseData<IPlatformHandle?>> PlatformHandleTestCases()
    {
        yield return new TestCaseData<IPlatformHandle?>(null, "NoHandle");
        yield return new TestCaseData<IPlatformHandle?>(new MockPlatformHandle(), "WithHandle");
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        VMProvider.Dispose();
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(PlatformHandleTestCases))]
    public async Task ViewModelTest(IPlatformHandle? platformHandle)
    {
        var viewModel = VMProvider.GetVideoViewModel() as MockVideoViewModel;
        viewModel!.CanPlay = true;
        var videoView = new VideoView
        {
            DataContext = viewModel
        };
        var window = new Window
        {
            Content = videoView
        };
        videoView.VideoViewer.Handle = platformHandle;
        window.Show();
        await Task.Delay(400);

        Assert.That(videoView.VideoViewer.MediaPlayer, Is.Not.Null);
        Assert.That(videoView.VideoViewer.MediaPlayer, Is.SameAs(viewModel.MediaPlayer));

        var mediaPlayer = viewModel.MediaPlayer as MockMediaPlayer;
        Assert.Multiple(() =>
        {
            Assert.That(mediaPlayer!.HasHandle(), Is.EqualTo(platformHandle is not null));
            Assert.That(viewModel.IsLoaded, Is.EqualTo(platformHandle is not null));
        });
    }

    [AvaloniaTest]
    public async Task ResizeWidthTest()
    {
        var initSize = new Size(1920, 1080);
        var newSize = new Size(1280, initSize.Height);

        // Initialize the view and window
        var videoView = new VideoView();
        var window = new Window
        {
            Width = initSize.Width,
            Height = initSize.Height,
            Content = videoView
        };
        window.Show();
        await Task.Delay(200);

        // Check that the view and window are initialized correctly
        Assert.That(videoView.Bounds.Height, Is.EqualTo(videoView.Bounds.Width * 9 / 16));

        // Resize the view
        var args = new SizeChangedEventArgs(null, null, initSize, newSize);
        videoView.OnVideoViewResize(videoView, args);

        // Check that the ratio is preserved
        Assert.That(videoView.Bounds.Height, Is.EqualTo(videoView.Bounds.Width * 9 / 16));
    }

    [AvaloniaTest]
    public async Task ResizeHeightTest()
    {
        var initSize = new Size(1920, 1080);
        var newSize = new Size(initSize.Width, 720);

        // Initialize the view and window
        var videoView = new VideoView();
        var window = new Window
        {
            Width = initSize.Width,
            Height = initSize.Height,
            Content = videoView
        };
        window.Show();
        await Task.Delay(200);

        // Check that the view and window are initialized correctly
        Assert.That(videoView.Bounds.Height, Is.EqualTo(videoView.Bounds.Width * 9 / 16));

        // Resize the view
        var args = new SizeChangedEventArgs(null, null, initSize, newSize);
        videoView.OnVideoViewResize(videoView, args);

        // Check that the ratio is preserved
        Assert.That(videoView.Bounds.Height, Is.EqualTo(videoView.Bounds.Width * 9 / 16));
    }

    //TODO Add tests for visibility
}
