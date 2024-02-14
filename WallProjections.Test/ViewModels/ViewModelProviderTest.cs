using System.Collections.Immutable;
using System.Reflection;
using Avalonia.Headless.NUnit;
using WallProjections.Models;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels.Editor;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Display;
using WallProjections.ViewModels.Editor;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Test.ViewModels;

[TestFixture]
[NonParallelizable]
public class ViewModelProviderTest
{
    [Test]
    public void SingletonPatternTest()
    {
        var instance1 = ViewModelProvider.Instance;
        var instance2 = ViewModelProvider.Instance;

        Assert.That(instance2, Is.SameAs(instance1));
    }

    #region Display

    [Test]
    public void GetDisplayViewModelTest()
    {
        var displayViewModel = ViewModelProvider.Instance.GetDisplayViewModel(new Config(new List<Hotspot>()));
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
        var imageViewModel = ViewModelProvider.Instance.GetImageViewModel();
        Assert.That(imageViewModel, Is.InstanceOf<ImageViewModel>());
        Assert.That(imageViewModel.Image, Is.Null);
    }

    [Test]
    public void GetVideoViewModelTest()
    {
        var videoViewModel = ViewModelProvider.Instance.GetVideoViewModel();
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
        var config = new Config(new List<Hotspot> { hotspot });
        var fileHandler = new MockFileHandler(config);
        var editorViewModel = ViewModelProvider.Instance.GetEditorViewModel(config, fileHandler);

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
        var fileHandler = new MockFileHandler(new Exception());
        var editorViewModel = ViewModelProvider.Instance.GetEditorViewModel(fileHandler);

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
        var editorHotspotViewModel = ViewModelProvider.Instance.GetEditorHotspotViewModel(hotspot);

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
        var editorHotspotViewModel = ViewModelProvider.Instance.GetEditorHotspotViewModel(id);

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
        var descriptionEditorViewModel = ViewModelProvider.Instance.GetDescriptionEditorViewModel();
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
        var imageEditorViewModel = ViewModelProvider.Instance.GetMediaEditorViewModel(MediaEditorType.Images);
        var videoEditorViewModel = ViewModelProvider.Instance.GetMediaEditorViewModel(MediaEditorType.Videos);

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
        Assert.That(
            () => ViewModelProvider.Instance.GetMediaEditorViewModel((MediaEditorType)2),
            Throws.InstanceOf<ArgumentOutOfRangeException>()
        );
    }

    [AvaloniaTest]
    [TestCase(MediaEditorType.Images, ImagePath, typeof(ImageThumbnailViewModel), TestName = "Images")]
    [TestCase(MediaEditorType.Videos, VideoPath, typeof(VideoThumbnailViewModel), TestName = "Videos")]
    public void GetThumbnailViewModelTest(MediaEditorType type, string filePath, Type expectedType)
    {
        const int row = 1;
        const int column = 2;
        var thumbnailViewModel = ViewModelProvider.Instance.GetThumbnailViewModel(type, filePath, row, column);

        Assert.Multiple(() =>
        {
            Assert.That(thumbnailViewModel, Is.InstanceOf(expectedType));
            Assert.That(thumbnailViewModel.FilePath, Is.EqualTo(filePath));
            Assert.That(thumbnailViewModel.Row, Is.EqualTo(row));
            Assert.That(thumbnailViewModel.Column, Is.EqualTo(column));
        });
    }

    [Test]
    public void GetThumbnailViewModelInvalidTypeTest()
    {
        Assert.That(
            () => ViewModelProvider.Instance.GetThumbnailViewModel((MediaEditorType)2, "", 0, 0),
            Throws.InstanceOf<ArgumentOutOfRangeException>()
        );
    }

    [Test]
    public void GetImportViewModelTest()
    {
        var descriptionEditorViewModel = new MockDescriptionEditorViewModel();
        var importViewModel = ViewModelProvider.Instance.GetImportViewModel(descriptionEditorViewModel);

        Assert.That(importViewModel, Is.InstanceOf<ImportViewModel>());
        Assert.That(importViewModel.DescriptionEditor, Is.EqualTo(descriptionEditorViewModel));
    }

    #endregion

    [Test]
    [NonParallelizable]
    public void UsingAfterDisposingTest()
    {
        ViewModelProvider.Instance.Dispose();
        Assert.That(ViewModelProvider.Instance, Is.Not.Null);
        GetVideoViewModelTest();
    }
}
