using System.Collections.Immutable;
using WallProjections.Helper.Interfaces;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks.Helper;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.ViewModels.Display;
using WallProjections.ViewModels.Display;
using WallProjections.ViewModels.Interfaces.Display;
using static WallProjections.Test.TestExtensions;

namespace WallProjections.Test.ViewModels.Display;

[TestFixture]
public class DisplayViewModelTest
{
    private const int HotspotId = 1;
    private const string Title = "Test";
    private const string Text = "test";

    private static readonly ImmutableList<string> ImagePaths =
        new List<string> { "test.png", "test2.jpg" }.ToImmutableList();

    private static ImmutableList<string> ImagePath => ImagePaths.GetRange(0, 1);

    private static readonly ImmutableList<string> VideoPaths =
        new List<string> { "test.mp4", "test2.mkv" }.ToImmutableList();

    private static ImmutableList<string> VideoPath => VideoPaths.GetRange(0, 1);

    #region MediaFiles

    private static ImmutableList<Hotspot.Media> FilesAll =>
        new List<Hotspot.Media>
        {
            new(1,
                Title,
                Text,
                ImagePaths,
                VideoPaths
            )
        }.ToImmutableList();

    private static ImmutableList<Hotspot.Media> FilesSingle =>
        new List<Hotspot.Media>
        {
            new(1,
                Title,
                Text,
                ImagePath,
                VideoPath
            )
        }.ToImmutableList();

    private static ImmutableList<Hotspot.Media> FilesNoVideo =>
        new List<Hotspot.Media>
        {
            new(1,
                Title,
                Text,
                ImagePaths,
                ImmutableList<string>.Empty
            )
        }.ToImmutableList();

    private static ImmutableList<Hotspot.Media> FilesNoImage =>
        new List<Hotspot.Media>
        {
            new(1,
                Title,
                Text,
                ImmutableList<string>.Empty,
                VideoPath
            )
        }.ToImmutableList();

    private static ImmutableList<Hotspot.Media> FilesNoMedia =>
        new List<Hotspot.Media>
        {
            new(1,
                Title,
                Text,
                ImmutableList<string>.Empty,
                ImmutableList<string>.Empty
            )
        }.ToImmutableList();

    #endregion

    private static MockViewModelProvider ViewModelProvider => new();

    private static IEnumerable<TestCaseData<ImmutableList<Hotspot.Media>>> CreationTestCases()
    {
        yield return MakeTestData(FilesAll, "AllMediaTypes");
        yield return MakeTestData(FilesSingle, "SingleMedia");
        yield return MakeTestData(FilesNoVideo, "NoVideo");
        yield return MakeTestData(FilesNoImage, "NoImage");
        yield return MakeTestData(FilesNoMedia, "NoMedia");
    }

    /// <summary>
    /// Test data for <see cref="DisplayViewModel"/>. Contains <see cref="Config"/> definition, and
    /// expected description, expected image files, and expected video files.
    /// </summary>
    /// <returns></returns>
    private static IEnumerable<TestCaseData<ImmutableList<Hotspot.Media>>> OnHotspotSelectedTestCases()
    {
        yield return MakeTestData(
            FilesAll,
            "AllMediaTypes"
        );
        yield return MakeTestData(
            FilesSingle,
            "SingleMedia"
        );
        yield return MakeTestData(
            FilesNoVideo,
            "NoVideo"
        );
        yield return MakeTestData(
            FilesNoImage,
            "NoImage"
        );
        yield return MakeTestData(
            FilesNoMedia,
            "NoMedia"
        );
    }

    private static IEnumerable<TestCaseData<(Exception, string)>> OnHotspotSelectedExceptionTestCases()
    {
        yield return MakeTestData(
            (new IConfig.HotspotNotFoundException(HotspotId) as Exception, DisplayViewModel.NotFound),
            "HotspotNotFound"
        );
        yield return MakeTestData(
            (new FileNotFoundException("File not found") as Exception, DisplayViewModel.NotFound),
            "FileNotFound"
        );
        yield return MakeTestData(
            (new Exception(), DisplayViewModel.GenericError),
            "GenericException"
        );
    }

    [Test]
    [TestCaseSource(nameof(CreationTestCases))]
    public void CreationTest(ImmutableList<Hotspot.Media> hotspots)
    {
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonEventHandler();
        var contentProvider = new MockContentProvider(hotspots);

        var displayViewModel = new DisplayViewModel(navigator, ViewModelProvider, contentProvider, pythonHandler);

        AssertJustInitialized(displayViewModel);
        displayViewModel.Dispose();
    }

    [Test]
    [TestCaseSource(nameof(OnHotspotSelectedTestCases))]
    public void OnHotspotSelectedTest(ImmutableList<Hotspot.Media> hotspots)
    {
        var hotspot = hotspots[0];
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonEventHandler();
        var contentProvider = new MockContentProvider(hotspots);

        var displayViewModel = new DisplayViewModel(navigator, ViewModelProvider, contentProvider, pythonHandler);
        var imageViewModel = (displayViewModel.ImageViewModel as MockImageViewModel)!;
        var videoViewModel = (displayViewModel.VideoViewModel as MockVideoViewModel)!;

        var args = new IPythonEventHandler.HotspotSelectedArgs(HotspotId);
        Assert.DoesNotThrow(() => displayViewModel.OnHotspotSelected(null, args));
        Assert.Multiple(() =>
        {
            Assert.That(displayViewModel.Description, Is.EqualTo(Text));

            Assert.That(imageViewModel.HideCount, Is.EqualTo(1));
            Assert.That(imageViewModel.ImagePaths, Is.SubsetOf(hotspot.ImagePaths));

            if (hotspot.ImagePaths.IsEmpty)
            {
                Assert.That(displayViewModel.ImageViewModel.HasImages, Is.False);
            }


            Assert.That(videoViewModel.StopCount, Is.EqualTo(1));
            Assert.That(videoViewModel.VideoPaths, Is.SubsetOf(hotspot.VideoPaths));

            if (hotspot.VideoPaths.IsEmpty)
            {
                Assert.That(displayViewModel.VideoViewModel.HasVideos, Is.False);
            }
        });
        displayViewModel.Dispose();
    }

    [Test]
    public void OnHotspotSelectedNoConfigTest()
    {
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonEventHandler();
        var contentProvider = new MockContentProvider(ImmutableList<Hotspot.Media>.Empty);

        var displayViewModel = new DisplayViewModel(navigator, ViewModelProvider, contentProvider, pythonHandler);

        var args = new IPythonEventHandler.HotspotSelectedArgs(HotspotId);
        AssertJustInitialized(displayViewModel);
        Assert.DoesNotThrow(() => displayViewModel.OnHotspotSelected(null, args));

        Assert.Multiple(() =>
        {
            Assert.That(displayViewModel.Description, Is.EqualTo(DisplayViewModel.NotFound));
            Assert.That(displayViewModel.ImageViewModel.HasImages, Is.False);
            Assert.That(displayViewModel.VideoViewModel.HasVideos, Is.False);
        });

        displayViewModel.Dispose();
    }

    [Test]
    [TestCaseSource(nameof(OnHotspotSelectedExceptionTestCases))]
    public void OnHotspotSelectedExceptionTest((Exception, string) testCase)
    {
        var (exception, expectedDescription) = testCase;
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonEventHandler();
        var contentProvider = new MockContentProvider(exception);

        var displayViewModel = new DisplayViewModel(navigator, ViewModelProvider, contentProvider, pythonHandler);
        var imageViewModel = (displayViewModel.ImageViewModel as MockImageViewModel)!;
        var videoViewModel = (displayViewModel.VideoViewModel as MockVideoViewModel)!;

        var args = new IPythonEventHandler.HotspotSelectedArgs(HotspotId);

        AssertJustInitialized(displayViewModel);

        Assert.DoesNotThrow(() => displayViewModel.OnHotspotSelected(null, args));
        Assert.Multiple(() =>
        {
            Assert.That(displayViewModel.Description, Is.EqualTo(expectedDescription));
            Assert.That(imageViewModel.HideCount, Is.EqualTo(1));
            Assert.That(videoViewModel.StopCount, Is.EqualTo(1));
        });
        displayViewModel.Dispose();
    }

    [Test]
    public void DisposeTest()
    {
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonEventHandler();
        var config = new Config(Enumerable.Empty<Hotspot>());
        var contentProvider = new ContentProvider(config);

        var displayViewModel = new DisplayViewModel(navigator, ViewModelProvider, contentProvider, pythonHandler);
        var videoViewModel = (displayViewModel.VideoViewModel as MockVideoViewModel)!;
        displayViewModel.Dispose();

        Assert.Multiple(() =>
        {
            Assert.That(pythonHandler.HasSubscribers, Is.False);
            Assert.That(videoViewModel.DisposeCount, Is.EqualTo(1));
        });
    }

    private static void AssertJustInitialized(IDisplayViewModel displayViewModel)
    {
        Assert.Multiple(() =>
        {
            Assert.That(displayViewModel.Description, Is.Empty);
            Assert.That(displayViewModel.ImageViewModel.HasImages, Is.False);
            Assert.That(displayViewModel.VideoViewModel.HasVideos, Is.False);
        });
    }
}
