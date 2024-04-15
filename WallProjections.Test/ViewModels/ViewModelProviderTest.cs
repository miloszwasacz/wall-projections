using System.Collections.Immutable;
using System.Reflection;
using WallProjections.Models;
using WallProjections.Test.Mocks;
using WallProjections.Test.Mocks.Helper;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.ViewModels.Display.Layouts;
using WallProjections.Test.Mocks.ViewModels.Editor;
using WallProjections.Test.Mocks.ViewModels.SecondaryScreens;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Display;
using WallProjections.ViewModels.Editor;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;
using WallProjections.ViewModels.SecondaryScreens;

namespace WallProjections.Test.ViewModels;

[TestFixture]
[NonParallelizable]
public class ViewModelProviderTest
{
    private static ViewModelProvider CreateViewModelProvider()
    {
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonHandler();
        var hotspotHandler = new MockHotspotHandler();
        var processProxy = new MockProcessProxy();
        var contentProvider = new MockContentProvider(Enumerable.Empty<Hotspot.Media>());
        var layoutProvider = new MockLayoutProvider();
        return new ViewModelProvider(
            navigator,
            pythonHandler,
            processProxy,
            _ => hotspotHandler,
            _ => contentProvider,
            () => layoutProvider,
            new MockLoggerFactory()
        );
    }

    #region Display

    [Test]
    public void GetDisplayViewModelTest()
    {
        using var vmProvider = CreateViewModelProvider();
        var displayViewModel = vmProvider.GetDisplayViewModel(new Config(new double[3, 3], new List<Hotspot>()));
        Assert.Multiple(() =>
        {
            Assert.That(displayViewModel, Is.InstanceOf<DisplayViewModel>());
            Assert.That(displayViewModel.ContentViewModel, Is.Not.Null);
        });
    }

    [Test]
    public void GetImageViewModelTest()
    {
        using var vmProvider = CreateViewModelProvider();
        var imageViewModel = vmProvider.GetImageViewModel();
        Assert.That(imageViewModel, Is.InstanceOf<ImageViewModel>());
        Assert.That(imageViewModel.Image, Is.Null);
    }

    [Test]
    public void GetVideoViewModelTest()
    {
        using var vmProvider = CreateViewModelProvider();
        var videoViewModel = vmProvider.GetVideoViewModel();
        Assert.That(videoViewModel, Is.InstanceOf<VideoViewModel>());
        Assert.That(videoViewModel.MediaPlayer, Is.Not.Null);
    }

    #endregion

    #region Editor

    private const string TestAssets = "Assets";
    private const string DescPath = "test.txt";
    private const string ImagePath = "test_image.png";
    private const string VideoPath = "test_video.mp4";
    private static readonly string DescContent = File.ReadAllText(Path.Combine(TestAssets, DescPath));

    private static Hotspot CreateHotspot(int id)
    {
        var hotspot = new Hotspot(
            id,
            new Coord(0, 0, 0),
            "Title",
            DescPath,
            ImmutableList.Create(ImagePath),
            ImmutableList.Create(VideoPath)
        );

        var baseDir = hotspot.GetType().GetField("_filePath", BindingFlags.NonPublic | BindingFlags.Instance)!;
        baseDir.SetValue(hotspot, TestAssets);

        return hotspot;
    }

    [AvaloniaTest]
    public void GetEditorViewModelTest()
    {
        var hotspot = CreateHotspot(0);
        var config = new Config(new double[3, 3], new List<Hotspot> { hotspot });
        var fileHandler = new MockFileHandler(config);
        using var vmProvider = CreateViewModelProvider();
        var editorViewModel = vmProvider.GetEditorViewModel(config, fileHandler);

        Assert.That(editorViewModel, Is.InstanceOf<EditorViewModel>());
        Assert.Multiple(() =>
        {
            Assert.That(editorViewModel.Hotspots, Has.Count.EqualTo(config.Hotspots.Count));
            Assert.That(editorViewModel.SelectedHotspot, Is.Not.Null);
        });
    }

    [AvaloniaTest]
    public void GetEmptyEditorViewModelTest()
    {
        var fileHandler = new MockFileHandler(new Exception());
        using var vmProvider = CreateViewModelProvider();
        var editorViewModel = vmProvider.GetEditorViewModel(fileHandler);

        Assert.That(editorViewModel, Is.InstanceOf<EditorViewModel>());
        Assert.Multiple(() =>
        {
            Assert.That(editorViewModel.Hotspots, Is.Empty);
            Assert.That(editorViewModel.SelectedHotspot, Is.Null);
        });
    }

    [AvaloniaTest]
    public void GetEditorHotspotViewModelTest()
    {
        var hotspot = CreateHotspot(1);
        using var vmProvider = CreateViewModelProvider();
        var editorHotspotViewModel = vmProvider.GetEditorHotspotViewModel(hotspot);

        Assert.That(editorHotspotViewModel, Is.InstanceOf<EditorHotspotViewModel>());
        Assert.Multiple(() =>
        {
            Assert.That(editorHotspotViewModel.Id, Is.EqualTo(hotspot.Id));
            Assert.That(editorHotspotViewModel.Position, Is.EqualTo(hotspot.Position));
            Assert.That(editorHotspotViewModel.Title, Is.EqualTo(hotspot.Title));
            Assert.That(editorHotspotViewModel.Description, Is.EqualTo(DescContent));
            Assert.That(editorHotspotViewModel.Images, Has.Count.EqualTo(hotspot.ImagePaths.Count));
            Assert.That(editorHotspotViewModel.Videos, Has.Count.EqualTo(hotspot.VideoPaths.Count));
        });
    }

    [Test]
    public void GetEditorHotspotViewModelFromIdTest()
    {
        const int id = 0;
        using var vmProvider = CreateViewModelProvider();
        var editorHotspotViewModel = vmProvider.GetEditorHotspotViewModel(id);

        Assert.That(editorHotspotViewModel, Is.InstanceOf<EditorHotspotViewModel>());
        Assert.Multiple(() =>
        {
            Assert.That(editorHotspotViewModel.Id, Is.EqualTo(id));
            Assert.That(editorHotspotViewModel.Position, Is.EqualTo(new Coord()));
            Assert.That(editorHotspotViewModel.Title, Is.Empty);
            Assert.That(editorHotspotViewModel.Description, Is.Empty);
            Assert.That(editorHotspotViewModel.Images, Is.Empty);
            Assert.That(editorHotspotViewModel.Videos, Is.Empty);
        });
    }

    [Test]
    public void GetDescriptionEditorViewModelTest()
    {
        using var vmProvider = CreateViewModelProvider();
        var descriptionEditorViewModel = vmProvider.GetDescriptionEditorViewModel();
        Assert.That(descriptionEditorViewModel, Is.InstanceOf<DescriptionEditorViewModel>());
        Assert.Multiple(() =>
        {
            Assert.That(descriptionEditorViewModel.Importer, Is.InstanceOf<IImportViewModel>());
            Assert.That(descriptionEditorViewModel.Title, Is.Empty);
            Assert.That(descriptionEditorViewModel.Description, Is.Empty);
        });
    }

    [Test]
    public void GetMediaEditorViewModelTest()
    {
        using var vmProvider = CreateViewModelProvider();
        var imageEditorViewModel = vmProvider.GetMediaEditorViewModel(MediaEditorType.Images);
        var videoEditorViewModel = vmProvider.GetMediaEditorViewModel(MediaEditorType.Videos);

        Assert.Multiple(() =>
        {
            Assert.That(imageEditorViewModel, Is.InstanceOf<MediaEditorViewModel>());
            Assert.That(imageEditorViewModel.Title, Is.EqualTo("Images"));
            Assert.That(imageEditorViewModel.Media, Is.Empty);
            Assert.That(imageEditorViewModel.SelectedMedia.Count, Is.EqualTo(0));

            Assert.That(videoEditorViewModel, Is.InstanceOf<MediaEditorViewModel>());
            Assert.That(videoEditorViewModel.Title, Is.EqualTo("Videos"));
            Assert.That(videoEditorViewModel.Media, Is.Empty);
            Assert.That(videoEditorViewModel.SelectedMedia.Count, Is.EqualTo(0));
        });
    }

    [Test]
    public void GetMediaEditorViewModelInvalidTypeTest()
    {
        using var vmProvider = CreateViewModelProvider();
        Assert.That(
            () => vmProvider.GetMediaEditorViewModel((MediaEditorType)2),
            Throws.InstanceOf<ArgumentOutOfRangeException>()
        );
    }

    [AvaloniaTest]
    [TestCase(MediaEditorType.Images, ImagePath, typeof(ImageThumbnailViewModel), TestName = "Images")]
    [TestCase(MediaEditorType.Videos, VideoPath, typeof(VideoThumbnailViewModel), TestName = "Videos")]
    public void GetThumbnailViewModelTest(MediaEditorType type, string filePath, Type expectedType)
    {
        using var vmProvider = CreateViewModelProvider();
        var thumbnailViewModel = vmProvider.GetThumbnailViewModel(type, filePath);

        Assert.Multiple(() =>
        {
            Assert.That(thumbnailViewModel, Is.InstanceOf(expectedType));
            Assert.That(thumbnailViewModel.FilePath, Is.EqualTo(filePath));
        });
    }

    [Test]
    public void GetThumbnailViewModelInvalidTypeTest()
    {
        using var vmProvider = CreateViewModelProvider();
        Assert.That(
            () => vmProvider.GetThumbnailViewModel((MediaEditorType)2, ""),
            Throws.InstanceOf<ArgumentOutOfRangeException>()
        );
    }

    [Test]
    public void GetImportViewModelTest()
    {
        var descriptionEditorViewModel = new MockDescriptionEditorViewModel();
        using var vmProvider = CreateViewModelProvider();
        var importViewModel = vmProvider.GetImportViewModel(descriptionEditorViewModel);

        Assert.That(importViewModel, Is.InstanceOf<ImportViewModel>());
        Assert.That(importViewModel.DescriptionEditor, Is.EqualTo(descriptionEditorViewModel));
    }

    #endregion

    #region Secondary screens

    [Test]
    public void GetSecondaryWindowViewModelTest()
    {
        using var vmProvider = CreateViewModelProvider();
        var secondaryWindowViewModel = vmProvider.GetSecondaryWindowViewModel();
        Assert.That(secondaryWindowViewModel, Is.InstanceOf<SecondaryWindowViewModel>());
        Assert.That(secondaryWindowViewModel.Content, Is.Null);
    }

    [Test]
    public void GetHotspotDisplayViewModelTest()
    {
        var hotspot = new Hotspot(
            0,
            new Coord(0, 0, 0),
            "Title",
            "test.txt",
            ImmutableList<string>.Empty,
            ImmutableList<string>.Empty
        );
        var config = new Config(new double[3, 3], new List<Hotspot> { hotspot });
        using var vmProvider = CreateViewModelProvider();

        var hotspotViewModel = vmProvider.GetHotspotDisplayViewModel(config);
        Assert.That(hotspotViewModel, Is.InstanceOf<HotspotDisplayViewModel>());
        Assert.Multiple(() =>
        {
            Assert.That(hotspotViewModel.Projections.Count(), Is.EqualTo(config.Hotspots.Count));
            //TODO Add this assertion when the hiding has been properly implemented
            // Assert.That(hotspotViewModel.IsVisible, Is.False);
        });
        Assert.That(
            hotspotViewModel.Projections,
            Is.EquivalentTo(config.Hotspots).Using<IHotspotProjectionViewModel, Hotspot>(
                (actual, expected) => actual.IsSameAsHotspot(expected)
            )
        );
    }

    [Test]
    public void GetHotspotProjectionViewModelTest()
    {
        var hotspot = CreateHotspot(0);
        using var vmProvider = CreateViewModelProvider();
        var hotspotProjectionViewModel = vmProvider.GetHotspotProjectionViewModel(hotspot);
        Assert.That(hotspotProjectionViewModel, Is.InstanceOf<HotspotProjectionViewModel>());
        Assert.Multiple(() =>
        {
            Assert.That(hotspotProjectionViewModel.Id, Is.EqualTo(hotspot.Id));
            Assert.That(hotspotProjectionViewModel.X, Is.EqualTo(hotspot.Position.X));
            Assert.That(hotspotProjectionViewModel.Y, Is.EqualTo(hotspot.Position.Y));
            Assert.That(hotspotProjectionViewModel.D, Is.EqualTo(hotspot.Position.R * 2));
            Assert.That(hotspotProjectionViewModel.IsActive, Is.False);
        });
    }

    [AvaloniaTest]
    public void GetArUcoGridViewModelTest()
    {
        using var vmProvider = CreateViewModelProvider();
        var arUcoGridViewModel = vmProvider.GetArUcoGridViewModel();
        Assert.That(arUcoGridViewModel, Is.InstanceOf<ArUcoGridViewModel>());
        Assert.That(arUcoGridViewModel.ArUcoList, Is.Not.Empty);
    }

    #endregion

    [Test]
    public void UsingAfterDisposingTest()
    {
        var config = new Config(new double[,] { }, Enumerable.Empty<Hotspot>());
        var vmProvider = CreateViewModelProvider();

        // Make sure that private instances of LibVLC and HotspotHandler are created
        AssertValidDisplayVM(vmProvider);
        AssertValidVideoVM(vmProvider);

        vmProvider.Dispose();
        Assert.That(vmProvider, Is.Not.Null);

        // Check if LibVLC and HotspotHandler are recreated
        AssertValidDisplayVM(vmProvider);
        AssertValidVideoVM(vmProvider);

        vmProvider.Dispose();
        return;

        // ReSharper disable once InconsistentNaming
        void AssertValidDisplayVM(IViewModelProvider viewModelProvider)
        {
            var displayViewModel = viewModelProvider.GetDisplayViewModel(config);
            Assert.That(displayViewModel, Is.InstanceOf<DisplayViewModel>());
            Assert.That(displayViewModel.ContentViewModel, Is.Not.Null);
        }

        // ReSharper disable once InconsistentNaming
        void AssertValidVideoVM(IViewModelProvider viewModelProvider)
        {
            var videoViewModel = viewModelProvider.GetVideoViewModel();
            Assert.That(videoViewModel, Is.InstanceOf<VideoViewModel>());
            Assert.That(videoViewModel.MediaPlayer, Is.Not.Null);
        }
    }
}
