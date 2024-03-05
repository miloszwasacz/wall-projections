using System.Collections.Immutable;
using System.Reflection;
using WallProjections.Models;
using WallProjections.Test.Mocks.Helper;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.ViewModels.Editor;
using WallProjections.Test.Mocks.ViewModels.SecondaryScreens;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Display;
using WallProjections.ViewModels.Editor;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;
using WallProjections.ViewModels.SecondaryScreens;

namespace WallProjections.Test.ViewModels;

[TestFixture]
[NonParallelizable]
public class ViewModelProviderTest
{
    #region Display

    [Test]
    public void GetDisplayViewModelTest()
    {
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonHandler();
        using var vmProvider = new ViewModelProvider(navigator, pythonHandler);
        var displayViewModel = vmProvider.GetDisplayViewModel(new Config(new double[3, 3], new List<Hotspot>()));
        Assert.Multiple(() =>
        {
            Assert.That(displayViewModel, Is.InstanceOf<DisplayViewModel>());
            Assert.That(displayViewModel.ImageViewModel, Is.Not.Null);
            Assert.That(displayViewModel.VideoViewModel, Is.Not.Null);
        });
    }

    [Test]
    public void GetImageViewModelTest()
    {
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonHandler();
        using var vmProvider = new ViewModelProvider(navigator, pythonHandler);
        var imageViewModel = vmProvider.GetImageViewModel();
        Assert.That(imageViewModel, Is.InstanceOf<ImageViewModel>());
        Assert.That(imageViewModel.Image, Is.Null);
    }

    [Test]
    public void GetVideoViewModelTest()
    {
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonHandler();
        using var vmProvider = new ViewModelProvider(navigator, pythonHandler);
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
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonHandler();
        var fileHandler = new MockFileHandler(config);
        using var vmProvider = new ViewModelProvider(navigator, pythonHandler);
        var editorViewModel = vmProvider.GetEditorViewModel(config, fileHandler);

        Assert.That(editorViewModel, Is.InstanceOf<EditorViewModel>());
        Assert.Multiple(() =>
        {
            Assert.That(editorViewModel.Hotspots, Has.Count.EqualTo(config.Hotspots.Count));
            Assert.That(editorViewModel.SelectedHotspot, Is.Not.Null);
        });
    }

    [Test]
    public void GetEmptyEditorViewModelTest()
    {
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonHandler();
        var fileHandler = new MockFileHandler(new Exception());
        using var vmProvider = new ViewModelProvider(navigator, pythonHandler);
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
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonHandler();
        using var vmProvider = new ViewModelProvider(navigator, pythonHandler);
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
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonHandler();
        using var vmProvider = new ViewModelProvider(navigator, pythonHandler);
        var editorHotspotViewModel = vmProvider.GetEditorHotspotViewModel(id);

        Assert.That(editorHotspotViewModel, Is.InstanceOf<EditorHotspotViewModel>());
        Assert.Multiple(() =>
        {
            Assert.That(editorHotspotViewModel.Id, Is.EqualTo(id));
            Assert.That(editorHotspotViewModel.Position, Is.EqualTo(new Coord(0, 0, 0)));
            Assert.That(editorHotspotViewModel.Title, Is.Empty);
            Assert.That(editorHotspotViewModel.Description, Is.Empty);
            Assert.That(editorHotspotViewModel.Images, Is.Empty);
            Assert.That(editorHotspotViewModel.Videos, Is.Empty);
        });
    }

    [Test]
    public void GetDescriptionEditorViewModelTest()
    {
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonHandler();
        using var vmProvider = new ViewModelProvider(navigator, pythonHandler);
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
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonHandler();
        using var vmProvider = new ViewModelProvider(navigator, pythonHandler);
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
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonHandler();
        using var vmProvider = new ViewModelProvider(navigator, pythonHandler);
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
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonHandler();
        using var vmProvider = new ViewModelProvider(navigator, pythonHandler);
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
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonHandler();
        using var vmProvider = new ViewModelProvider(navigator, pythonHandler);
        Assert.That(
            () => vmProvider.GetThumbnailViewModel((MediaEditorType)2, ""),
            Throws.InstanceOf<ArgumentOutOfRangeException>()
        );
    }

    [Test]
    public void GetImportViewModelTest()
    {
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonHandler();
        var descriptionEditorViewModel = new MockDescriptionEditorViewModel();
        using var vmProvider = new ViewModelProvider(navigator, pythonHandler);
        var importViewModel = vmProvider.GetImportViewModel(descriptionEditorViewModel);

        Assert.That(importViewModel, Is.InstanceOf<ImportViewModel>());
        Assert.That(importViewModel.DescriptionEditor, Is.EqualTo(descriptionEditorViewModel));
    }

    #endregion

    #region Secondary screens

    [Test]
    public void GetSecondaryWindowViewModelTest()
    {
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonHandler();
        using var vmProvider = new ViewModelProvider(navigator, pythonHandler);
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
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonHandler();
        using var vmProvider = new ViewModelProvider(navigator, pythonHandler);

        var hotspotViewModel = vmProvider.GetHotspotDisplayViewModel(config);
        Assert.That(hotspotViewModel, Is.InstanceOf<HotspotDisplayViewModel>());
        Assert.Multiple(() =>
        {
            Assert.That(hotspotViewModel.Projections, Has.Count.EqualTo(config.Hotspots.Count));
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
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonHandler();
        using var vmProvider = new ViewModelProvider(navigator, pythonHandler);
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
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonHandler();
        using var vmProvider = new ViewModelProvider(navigator, pythonHandler);
        var arUcoGridViewModel = vmProvider.GetArUcoGridViewModel();
        Assert.That(arUcoGridViewModel, Is.InstanceOf<ArUcoGridViewModel>());
        Assert.That(arUcoGridViewModel.ArUcoList, Is.Not.Empty);
    }

    #endregion

    [Test]
    public void UsingAfterDisposingTest()
    {
        var navigator = new MockNavigator();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new ViewModelProvider(navigator, pythonHandler);
        vmProvider.Dispose();
        Assert.That(vmProvider, Is.Not.Null);
        var videoViewModel = vmProvider.GetVideoViewModel();
        Assert.That(videoViewModel, Is.InstanceOf<VideoViewModel>());
        Assert.That(videoViewModel.MediaPlayer, Is.Not.Null);
        vmProvider.Dispose();
    }
}
