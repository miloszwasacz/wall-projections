using System.Collections.Immutable;
using WallProjections.Helper.Interfaces;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks;
using WallProjections.Test.Mocks.Helper;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.ViewModels.Display.Layouts;
using WallProjections.ViewModels.Display;
using WallProjections.ViewModels.Display.Layouts;
using WallProjections.ViewModels.Interfaces.Display.Layouts;
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

    private static ILayoutProvider LayoutProvider => new MockLayoutProvider();

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
    private static IEnumerable<TestCaseData<ImmutableList<Hotspot.Media>>> OnHotspotActivatedTestCases()
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

    private static IEnumerable<TestCaseData<(Exception, string)>> OnHotspotActivatedExceptionTestCases()
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
        var contentProvider = new MockContentProvider(hotspots);
        var hotspotHandler = new MockHotspotHandler();

        using var displayViewModel = new DisplayViewModel(
            navigator,
            ViewModelProvider,
            contentProvider,
            LayoutProvider,
            hotspotHandler,
            new MockLoggerFactory()
        );

        AssertJustInitialized(displayViewModel);
    }

    [Test]
    [TestCaseSource(nameof(OnHotspotActivatedTestCases))]
    [Timeout(10000)]
    public async Task OnHotspotActivatingTest(ImmutableList<Hotspot.Media> hotspots)
    {
        var hotspot = hotspots[0];
        var navigator = new MockNavigator();
        var contentProvider = new MockContentProvider(hotspots);
        var hotspotHandler = new MockHotspotHandler();

        using var displayViewModel = new DisplayViewModel(
            navigator,
            ViewModelProvider,
            contentProvider,
            LayoutProvider,
            hotspotHandler,
            new MockLoggerFactory()
        );

        hotspotHandler.StartHotspotActivation(hotspot.Id);
        await Task.Delay(200);
        Assert.That(displayViewModel.ContentViewModel, Is.Not.InstanceOf<MockGenericLayout>());

        hotspotHandler.DeactivateHotspot(hotspot.Id);
        await Task.Delay(200);
        Assert.That(displayViewModel.ContentViewModel, Is.Not.InstanceOf<MockGenericLayout>());

        hotspotHandler.StartHotspotActivation(hotspot.Id);
        await Task.Delay(500);
        hotspotHandler.ActivateHotspot(hotspot.Id);
        await Task.Delay(200);
        Assert.That(displayViewModel.ContentViewModel, Is.InstanceOf<MockGenericLayout>());

        // No new content should be created if the hotspot is already active
        var oldContent = displayViewModel.ContentViewModel;
        hotspotHandler.ActivateHotspot(hotspot.Id);
        await Task.Delay(200);
        Assert.That(displayViewModel.ContentViewModel, Is.SameAs(oldContent));
    }

    [Test]
    [TestCaseSource(nameof(OnHotspotActivatedTestCases))]
    [Timeout(10000)]
    public async Task OnHotspotActivatedTest(ImmutableList<Hotspot.Media> hotspots)
    {
        var hotspot = hotspots[0];
        var hotspot2 = new Hotspot.Media(2, "Test2", "Test2", ImmutableList<string>.Empty, ImmutableList<string>.Empty);
        var navigator = new MockNavigator();
        var contentProvider = new MockContentProvider(hotspots.Add(hotspot2));
        var hotspotHandler = new MockHotspotHandler();

        using var displayViewModel = new DisplayViewModel(
            navigator,
            ViewModelProvider,
            contentProvider,
            LayoutProvider,
            hotspotHandler,
            new MockLoggerFactory()
        );

        var args = new IHotspotHandler.HotspotArgs(hotspot.Id);
        Assert.DoesNotThrowAsync(async () =>
        {
            displayViewModel.OnHotspotActivated(null, args);
            await Task.Delay(200);
        });
        Assert.That(displayViewModel.ContentViewModel, Is.InstanceOf<MockGenericLayout>());
        var content = (MockGenericLayout)displayViewModel.ContentViewModel;
        Assert.Multiple(() =>
        {
            Assert.That(content.Media, Is.EqualTo(hotspot));
            Assert.That(content.IsDisposed, Is.False);
        });

        var args2 = new IHotspotHandler.HotspotArgs(hotspot2.Id);
        Assert.DoesNotThrowAsync(async () =>
        {
            displayViewModel.OnHotspotActivated(null, args2);
            await Task.Delay(200);
        });
        Assert.That(displayViewModel.ContentViewModel, Is.InstanceOf<MockGenericLayout>());
        var content2 = (MockGenericLayout)displayViewModel.ContentViewModel;
        Assert.Multiple(() =>
        {
            Assert.That(content2.Media, Is.EqualTo(hotspot2));
            Assert.That(content2.IsDisposed, Is.False);
            Assert.That(content.IsDisposed, Is.True);
        });

        hotspotHandler.ActivateHotspot(hotspot.Id);
        await Task.Delay(200);
        Assert.That(displayViewModel.ContentViewModel, Is.InstanceOf<MockGenericLayout>());
        var content3 = (MockGenericLayout)displayViewModel.ContentViewModel;
        Assert.Multiple(() =>
        {
            Assert.That(content3.Media, Is.EqualTo(hotspot));
            Assert.That(content3.IsDisposed, Is.False);
            Assert.That(content2.IsDisposed, Is.True);
        });
    }

    [Test]
    [Timeout(2000)]
    public void OnHotspotActivatedNonexistentHotspotTest()
    {
        var navigator = new MockNavigator();
        var contentProvider = new MockContentProvider(ImmutableList<Hotspot.Media>.Empty);
        var hotspotHandler = new MockHotspotHandler();

        using var displayViewModel = new DisplayViewModel(
            navigator,
            ViewModelProvider,
            contentProvider,
            LayoutProvider,
            hotspotHandler,
            new MockLoggerFactory()
        );

        var args = new IHotspotHandler.HotspotArgs(HotspotId);
        AssertJustInitialized(displayViewModel);

        Assert.DoesNotThrowAsync(async () =>
        {
            displayViewModel.OnHotspotActivated(null, args);
            await Task.Delay(200);
        });
        Assert.That(displayViewModel.ContentViewModel, Is.InstanceOf<MockSimpleDescriptionLayout>());
        var content = (MockSimpleDescriptionLayout)displayViewModel.ContentViewModel;
        Assert.Multiple(() =>
        {
            Assert.That(content.Title, Is.EqualTo("Error"));
            Assert.That(content.Description, Is.EqualTo(DisplayViewModel.NotFound));
        });
    }

    [Test]
    [TestCaseSource(nameof(OnHotspotActivatedExceptionTestCases))]
    [Timeout(5000)]
    public async Task OnHotspotActivatedExceptionTest((Exception, string) testCase)
    {
        var (exception, expectedDescription) = testCase;
        var navigator = new MockNavigator();
        var contentProvider = new MockContentProvider(exception);
        var hotspotHandler = new MockHotspotHandler();

        using var displayViewModel = new DisplayViewModel(
            navigator,
            ViewModelProvider,
            contentProvider,
            LayoutProvider,
            hotspotHandler,
            new MockLoggerFactory()
        );

        var args = new IHotspotHandler.HotspotArgs(HotspotId);
        AssertJustInitialized(displayViewModel);

        Assert.DoesNotThrow(() => displayViewModel.OnHotspotActivated(null, args));
        await Task.Delay(200);

        Assert.That(displayViewModel.ContentViewModel, Is.InstanceOf<MockSimpleDescriptionLayout>());
        var content = (MockSimpleDescriptionLayout)displayViewModel.ContentViewModel;
        Assert.Multiple(() =>
        {
            Assert.That(content.Title, Is.EqualTo("Error"));
            Assert.That(content.Description, Is.EqualTo(expectedDescription));
        });
    }

    [Test]
    [Timeout(5000)]
    public async Task OpenEditorTest()
    {
        var navigator = new MockNavigator();
        var contentProvider = new MockContentProvider(FilesAll);
        var hotspotHandler = new MockHotspotHandler();
        Assert.That(navigator.IsEditorOpen, Is.False);

        using var displayViewModel = new DisplayViewModel(
            navigator,
            ViewModelProvider,
            contentProvider,
            LayoutProvider,
            hotspotHandler,
            new MockLoggerFactory()
        );
        await displayViewModel.OpenEditor();

        Assert.That(navigator.IsEditorOpen, Is.True);
    }

    [Test]
    public void CloseDisplayTest()
    {
        var navigator = new MockNavigator();
        var contentProvider = new MockContentProvider(FilesAll);
        var hotspotHandler = new MockHotspotHandler();
        Assert.That(navigator.HasBeenShutDown, Is.False);

        using var displayViewModel = new DisplayViewModel(
            navigator,
            ViewModelProvider,
            contentProvider,
            LayoutProvider,
            hotspotHandler,
            new MockLoggerFactory()
        );
        displayViewModel.CloseDisplay();

        Assert.That(navigator.HasBeenShutDown, Is.True);
    }

    [Test]
    public async Task DisposeTest()
    {
        var hotspots = FilesAll;
        var hotspot = hotspots[0];
        var navigator = new MockNavigator();
        var contentProvider = new MockContentProvider(FilesAll);
        var hotspotHandler = new MockHotspotHandler();

        var displayViewModel = new DisplayViewModel(
            navigator,
            ViewModelProvider,
            contentProvider,
            LayoutProvider,
            hotspotHandler,
            new MockLoggerFactory()
        );
        displayViewModel.OnHotspotActivated(null, new IHotspotHandler.HotspotArgs(hotspot.Id));
        await Task.Delay(200);

        Assert.That(displayViewModel.ContentViewModel, Is.InstanceOf<MockGenericLayout>());
        var content = (MockGenericLayout)displayViewModel.ContentViewModel;
        displayViewModel.Dispose();

        Assert.That(displayViewModel.ContentViewModel, Is.InstanceOf<MockSimpleDescriptionLayout>());
        AssertJustInitialized(displayViewModel);
        Assert.Multiple(() =>
        {
            Assert.That(hotspotHandler.HasActivatedSubscribers, Is.False);
            Assert.That(content.IsDisposed, Is.True);
        });
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    /// <summary>
    /// Asserts that the provided <see cref="DisplayViewModel"/> is in a just-initialized state.
    /// </summary>
    private static void AssertJustInitialized(DisplayViewModel displayViewModel)
    {
        Assert.That(displayViewModel.ContentViewModel, Is.InstanceOf<MockSimpleDescriptionLayout>());
        var content = (MockSimpleDescriptionLayout)displayViewModel.ContentViewModel;
        Assert.Multiple(() =>
        {
            Assert.That(content.Title, Is.EqualTo(WelcomeViewModel.WelcomeTitle));
            Assert.That(content.Description, Is.EqualTo(WelcomeViewModel.WelcomeMessage));
        });
    }
}
