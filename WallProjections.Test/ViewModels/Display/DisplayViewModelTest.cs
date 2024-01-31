using WallProjections.Helper.Interfaces;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks.Helper;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.ViewModels.Display;
using WallProjections.ViewModels.Interfaces.Display;
using static WallProjections.Test.TestExtensions;

namespace WallProjections.Test.ViewModels.Display;

[TestFixture]
public class DisplayViewModelTest
{
    private const int HotspotId = 1;
    private const string Text = "test";
    private const string ImagePath = "test.png";
    private const string VideoPath = "test.mp4";

    #region MediaFiles

    private static List<Hotspot.Media> FilesAll =>
        new() { new Hotspot.Media(Text, ImagePath, VideoPath) };

    private static List<Hotspot.Media> FilesNoVideo =>
        new() { new Hotspot.Media(Text, ImagePath) };

    private static List<Hotspot.Media> FilesNoImage => new()
        { new Hotspot.Media(Text, VideoPath: VideoPath) };

    private static List<Hotspot.Media> FilesNoMedia => new() { new Hotspot.Media(Text) };

    #endregion

    private static MockViewModelProvider ViewModelProvider => new();

    private static IEnumerable<TestCaseData<List<Hotspot.Media>>> CreationTestCases()
    {
        yield return MakeTestData(FilesAll, "AllMediaTypes");
        yield return MakeTestData(FilesNoVideo, "NoVideo");
        yield return MakeTestData(FilesNoImage, "NoImage");
        yield return MakeTestData(FilesNoMedia, "NoMedia");
    }

    private static IEnumerable<TestCaseData<(List<Hotspot.Media>, string[]?, string[]?)>> OnHotspotSelectedTestCases()
    {
        yield return MakeTestData<(List<Hotspot.Media>, string[]?, string[]?)>(
            (FilesAll, new[] { ImagePath }, new[] { VideoPath }),
            "AllMediaTypes"
        );
        yield return MakeTestData<(List<Hotspot.Media>, string[]?, string[]?)>(
            (FilesNoVideo, new[] { ImagePath }, null),
            "NoVideo"
        );
        yield return MakeTestData<(List<Hotspot.Media>, string[]?, string[]?)>(
            (FilesNoImage, null, new[] { VideoPath }),
            "NoImage"
        );
        yield return MakeTestData<(List<Hotspot.Media>, string[]?, string[]?)>(
            (FilesNoMedia, null, null),
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
    public void CreationTest(List<Hotspot.Media> files)
    {
        var pythonHandler = new MockPythonEventHandler();
        var config = new Config(Enumerable.Empty<Hotspot>());
        var displayViewModel = new DisplayViewModel(ViewModelProvider, config, pythonHandler);

        AssertJustInitialized(displayViewModel);
        displayViewModel.Dispose();
    }

    [Test]
    [TestCaseSource(nameof(OnHotspotSelectedTestCases))]
    public void OnHotspotSelectedTest((List<Hotspot.Media>, string[]?, string[]?) testCase)
    {
        var (files, expectedImagePaths, expectedVideoPaths) = testCase;
        var pythonHandler = new MockPythonEventHandler();
        var config = new Config(Enumerable.Empty<Hotspot>());

        var displayViewModel = new DisplayViewModel(ViewModelProvider, config, pythonHandler);
        var imageViewModel = (displayViewModel.ImageViewModel as MockImageViewModel)!;
        var videoViewModel = (displayViewModel.VideoViewModel as MockVideoViewModel)!;

        var args = new IPythonEventHandler.HotspotSelectedArgs(HotspotId);
        Assert.DoesNotThrow(() => displayViewModel.OnHotspotSelected(null, args));
        Assert.Multiple(() =>
        {
            Assert.That(displayViewModel.Description, Is.EqualTo(Text));

            Assert.That(imageViewModel.HideCount, Is.EqualTo(1));
            if (expectedImagePaths is not null)
                Assert.That(imageViewModel.ImagePaths, Is.EquivalentTo(expectedImagePaths));
            else
                Assert.That(displayViewModel.ImageViewModel.HasImages, Is.False);

            Assert.That(videoViewModel.StopCount, Is.EqualTo(1));
            if (expectedVideoPaths is not null && expectedImagePaths is null)
                Assert.That(videoViewModel.VideoPaths, Is.EquivalentTo(expectedVideoPaths));
            else
                Assert.That(displayViewModel.VideoViewModel.HasVideos, Is.False);
        });
        displayViewModel.Dispose();
    }

    [Test]
    public void OnHotspotSelectedNoConfigTest()
    {
        var pythonHandler = new MockPythonEventHandler();
        var config = new Config(Enumerable.Empty<Hotspot>());

        var displayViewModel = new DisplayViewModel(ViewModelProvider, config, pythonHandler);

        var args = new IPythonEventHandler.HotspotSelectedArgs(HotspotId);
        Assert.DoesNotThrow(() => displayViewModel.OnHotspotSelected(null, args));
        AssertJustInitialized(displayViewModel);
        displayViewModel.Dispose();
    }

    [Test]
    [TestCaseSource(nameof(OnHotspotSelectedExceptionTestCases))]
    public void OnHotspotSelectedExceptionTest((Exception, string) testCase)
    {
        var (exception, expectedDescription) = testCase;
        var pythonHandler = new MockPythonEventHandler();
        var config = new Config(Enumerable.Empty<Hotspot>());

        var displayViewModel = new DisplayViewModel(ViewModelProvider, config, pythonHandler);
        var imageViewModel = (displayViewModel.ImageViewModel as MockImageViewModel)!;
        var videoViewModel = (displayViewModel.VideoViewModel as MockVideoViewModel)!;

        var args = new IPythonEventHandler.HotspotSelectedArgs(HotspotId);
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
        var pythonHandler = new MockPythonEventHandler();
        var config = new Config(Enumerable.Empty<Hotspot>());

        var displayViewModel = new DisplayViewModel(ViewModelProvider, config, pythonHandler);
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
