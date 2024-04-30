using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.ViewModels.Display;
using WallProjections.Views.Converters;
using WallProjections.Views.Display;
using static WallProjections.Test.TestExtensions;

namespace WallProjections.Test.Views.Display;

[TestFixture]
public class VideoViewTest
{
    private const double AspectRatioTolerance = 0.002;

    // ReSharper disable once InconsistentNaming
    private readonly MockViewModelProvider VMProvider = new();

    private static IEnumerable<TestCaseData<IPlatformHandle?>> PlatformHandleTestCases()
    {
        yield return new TestCaseData<IPlatformHandle?>(null, "NoHandle");
        yield return new TestCaseData<IPlatformHandle?>(new MockPlatformHandle(), "WithHandle");
    }

    private static IEnumerable<TestCaseData<(uint, uint)>> AspectRatioTestCases()
    {
        yield return new TestCaseData<(uint, uint)>((1920, 1080), "1920x1080(16:9)");
        yield return new TestCaseData<(uint, uint)>((1680, 720), "1680x720(21:9)");
        yield return new TestCaseData<(uint, uint)>((480, 720), "720x1080(2:3)");
        yield return new TestCaseData<(uint, uint)>((540, 540), "540x540(1:1)");
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
    [TestCaseSource(nameof(AspectRatioTestCases))]
    public async Task ResizeWidthTest((uint Width, uint Height) video)
    {
        var initSize = new Size(1920, 1080);
        var newSize = new Size(1280, initSize.Height);

        // Initialize the view and window
        var mediaPlayer = new MockMediaPlayer
        {
            VideoSize = video
        };
        var videoView = new VideoView
        {
            DataContext = new MockVideoViewModel(mediaPlayer)
            {
                CanPlay = true
            }
        };
        var window = new Window
        {
            Width = initSize.Width,
            Height = initSize.Height,
            Content = videoView
        };
        videoView.VideoViewer.Handle = new MockPlatformHandle();
        window.Show();
        await Task.Delay(400);

        // Check that the view and window are initialized correctly
        AssertAspectRatio(videoView, video.Width, video.Height);

        // Resize the view
        window.Width = newSize.Width;
        await Task.Delay(200);

        // Check that the ratio is preserved
        AssertAspectRatio(videoView, video.Width, video.Height);
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(AspectRatioTestCases))]
    public async Task ResizeHeightTest((uint Width, uint Height) video)
    {
        var initSize = new Size(1920, 1080);
        var newSize = new Size(initSize.Width, 720);

        // Initialize the view and window
        var mediaPlayer = new MockMediaPlayer
        {
            VideoSize = video
        };
        var videoView = new VideoView
        {
            DataContext = new MockVideoViewModel(mediaPlayer)
            {
                CanPlay = true
            }
        };
        var window = new Window
        {
            Width = initSize.Width,
            Height = initSize.Height,
            Content = videoView
        };
        videoView.VideoViewer.Handle = new MockPlatformHandle();
        window.Show();
        await Task.Delay(400);

        // Check that the view and window are initialized correctly
        AssertAspectRatio(videoView, video.Width, video.Height);

        // Resize the view
        window.Height = newSize.Height;
        await Task.Delay(200);

        // Check that the ratio is preserved
        AssertAspectRatio(videoView, video.Width, video.Height);
    }

    [TestFixture]
    public class DefaultAspectRatioTest
    {
        [AvaloniaTest]
        public async Task NoMediaPlayer()
        {
            var initSize = new Size(1920, 1080);
            var newSize = new Size(1280, 720);
            var expectedVideoWidth = AspectRatioConverter.DefaultAspectRatio.Width;
            var expectedVideoHeight = AspectRatioConverter.DefaultAspectRatio.Height;

            // Initialize the view and window
            var mediaPlayer = new MockMediaPlayer
            {
                // The video size is not 16:9
                VideoSize = (540, 540)
            };
            var videoView = new VideoView
            {
                DataContext = new MockVideoViewModel(mediaPlayer)
            };
            var window = new Window
            {
                Width = initSize.Width,
                Height = initSize.Height,
                Content = videoView
            };
            window.Show();
            await Task.Delay(400);

            // Check that the view and window are initialized correctly
            AssertAspectRatio(videoView, expectedVideoWidth, expectedVideoHeight);

            // Resize the view
            window.Width = newSize.Width;
            window.Height = newSize.Height;
            await Task.Delay(200);

            // Check that the ratio is still the default
            AssertAspectRatio(videoView, expectedVideoWidth, expectedVideoHeight);
        }

        [AvaloniaTest]
        public async Task NoVideoSize()
        {
            var initSize = new Size(1920, 1080);
            var newSize = new Size(1280, 720);
            var expectedVideoWidth = AspectRatioConverter.DefaultAspectRatio.Width;
            var expectedVideoHeight = AspectRatioConverter.DefaultAspectRatio.Height;

            // Initialize the view and window
            var mediaPlayer = new MockMediaPlayer();
            var videoView = new VideoView
            {
                DataContext = new MockVideoViewModel(mediaPlayer)
                {
                    CanPlay = true
                }
            };
            var window = new Window
            {
                Width = initSize.Width,
                Height = initSize.Height,
                Content = videoView
            };
            videoView.VideoViewer.Handle = new MockPlatformHandle();
            window.Show();
            await Task.Delay(400);

            // Check that the view and window are initialized correctly
            AssertAspectRatio(videoView, expectedVideoWidth, expectedVideoHeight);

            // Resize the view
            window.Width = newSize.Width;
            window.Height = newSize.Height;
            await Task.Delay(200);

            // Check that the ratio is still the default
            AssertAspectRatio(videoView, expectedVideoWidth, expectedVideoHeight);
        }
    }

    //TODO Add tests for visibility

    /// <summary>
    /// Asserts that the aspect ratio of the <paramref name="videoView" />.<see cref="VideoView.VideoViewer" />
    /// is the same as the video's aspect ratio (within <see cref="AspectRatioTolerance"/>)
    /// </summary>
    /// <param name="videoView">The video view to check</param>
    /// <param name="videoWidth">The width of the video</param>
    /// <param name="videoHeight">The height of the video</param>
    private static void AssertAspectRatio(VideoView videoView, double videoWidth, double videoHeight)
    {
        var actualSize = videoView.VideoViewer.Bounds;
        var actualAspectRatio = actualSize.Width / actualSize.Height;
        var expectedAspectRatio = videoWidth / videoHeight;
        Assert.That(actualAspectRatio, Is.EqualTo(expectedAspectRatio).Within(AspectRatioTolerance));
    }
}
