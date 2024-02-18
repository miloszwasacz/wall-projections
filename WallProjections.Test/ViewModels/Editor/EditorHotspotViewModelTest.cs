using System.Collections.Immutable;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using WallProjections.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.ViewModels.Editor;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Test.ViewModels.Editor;

[TestFixture]
public class EditorHotspotViewModelTest
{
    private static readonly string TestAssets = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets");

    private static Hotspot CreateHotspot()
    {
        const int id = 1;
        var position = new Coord(0, 0, 0);
        const string title = "Title";
        var descriptionPath = Path.Combine(TestAssets, "test.txt");
        var images = new[] { "test_image.png", "test_image_2.png" }
            .Select(x => Path.Combine(TestAssets, x))
            .ToImmutableList();
        var videos = new[] { "test_video.mp4" }
            .Select(x => Path.Combine(TestAssets, x))
            .ToImmutableList();

        return new Hotspot(id, position, title, descriptionPath, images, videos);
    }

    /// <summary>
    /// Uses <see cref="Window.StorageProvider" /> to get a file from the test assets.
    /// </summary>
    private static async Task<IStorageFile> GetFile(string fileName)
    {
        var window = new Window();
        var path = Path.Combine(TestAssets, fileName);
        var uri = new Uri($"file://{path}");

        return await window.StorageProvider.TryGetFileFromPathAsync(uri)
               ?? throw new FileNotFoundException("Could not find the file.", uri.AbsolutePath);
    }

    [AvaloniaTest]
    public void IdConstructorTest()
    {
        const int id = 1;
        var position = new Coord(0, 0, 0);
        IEditorHotspotViewModel editorHotspotViewModel = new EditorHotspotViewModel(id, new MockViewModelProvider());

        Assert.Multiple(() =>
        {
            Assert.That(editorHotspotViewModel.Id, Is.EqualTo(id));
            Assert.That(editorHotspotViewModel.Position, Is.EqualTo(position));
            Assert.That(editorHotspotViewModel.Title, Is.Empty);
            Assert.That(editorHotspotViewModel.Description, Is.Empty);
            Assert.That(editorHotspotViewModel.Images, Is.Empty);
            Assert.That(editorHotspotViewModel.Videos, Is.Empty);
            Assert.That(editorHotspotViewModel.FallbackTitle, Is.EqualTo($"Hotspot {id}"));
            Assert.That(editorHotspotViewModel.IsFallback, Is.True);
        });
    }

    [AvaloniaTest]
    public void HotspotConstructorTest()
    {
        var hotspot = CreateHotspot();
        var description = File.ReadAllText(hotspot.DescriptionPath);

        IEditorHotspotViewModel editorHotspotViewModel =
            new EditorHotspotViewModel(hotspot, new MockViewModelProvider());

        Assert.Multiple(() =>
        {
            Assert.That(editorHotspotViewModel.Id, Is.EqualTo(hotspot.Id));
            Assert.That(editorHotspotViewModel.Position, Is.EqualTo(hotspot.Position));
            Assert.That(editorHotspotViewModel.Title, Is.EqualTo(hotspot.Title));
            Assert.That(editorHotspotViewModel.Description, Is.EqualTo(description));
            Assert.That(editorHotspotViewModel.Images.Select(x => x.FilePath), Is.EquivalentTo(hotspot.ImagePaths));
            Assert.That(editorHotspotViewModel.Videos.Select(x => x.FilePath), Is.EquivalentTo(hotspot.VideoPaths));
            Assert.That(editorHotspotViewModel.FallbackTitle, Is.EqualTo($"Hotspot {hotspot.Id}"));
            Assert.That(editorHotspotViewModel.IsFallback, Is.False);
        });
    }

    [AvaloniaTest]
    public void SetterTest()
    {
        var hotspot = CreateHotspot();
        var originalDescription = File.ReadAllText(hotspot.DescriptionPath);

        var position = new Coord(0, 0, 0);
        const string title = "New Title";
        const string description = "New Description";

        IEditorHotspotViewModel editorHotspotViewModel =
            new EditorHotspotViewModel(hotspot, new MockViewModelProvider());

        Assert.Multiple(() =>
        {
            Assert.That(editorHotspotViewModel.Position, Is.EqualTo(hotspot.Position));
            Assert.That(editorHotspotViewModel.Title, Is.EqualTo(hotspot.Title));
            Assert.That(editorHotspotViewModel.Description, Is.EqualTo(originalDescription));
        });

        editorHotspotViewModel.Position = position;
        editorHotspotViewModel.Title = title;
        editorHotspotViewModel.Description = description;

        Assert.Multiple(() =>
        {
            Assert.That(editorHotspotViewModel.Position, Is.EqualTo(position));
            Assert.That(editorHotspotViewModel.Title, Is.EqualTo(title));
            Assert.That(editorHotspotViewModel.Description, Is.EqualTo(description));
        });
    }

    [AvaloniaTest]
    public async Task AddMediaTest()
    {
        var hotspot = CreateHotspot();
        var editorHotspotViewModel = new EditorHotspotViewModel(hotspot, new MockViewModelProvider());
        editorHotspotViewModel.Images.Clear();
        editorHotspotViewModel.Videos.Clear();

        var image = await GetFile("test_image.png");
        var video = await GetFile("test_video.mp4");

        editorHotspotViewModel.AddMedia(MediaEditorType.Images, new[] { image });
        editorHotspotViewModel.AddMedia(MediaEditorType.Videos, new[] { video });

        Assert.Multiple(() =>
        {
            Assert.That(
                editorHotspotViewModel.Images.Where(x => x.FilePath == image.Path.AbsolutePath),
                Has.Exactly(1).Items
            );
            Assert.That(
                editorHotspotViewModel.Videos.Where(x => x.FilePath == video.Path.AbsolutePath),
                Has.Exactly(1).Items
            );
        });

        Assert.Multiple(() =>
        {
            Assert.That(editorHotspotViewModel.Images[0].FilePath, Is.EqualTo(image.Path.AbsolutePath));
            Assert.That(editorHotspotViewModel.Videos[0].FilePath, Is.EqualTo(video.Path.AbsolutePath));
        });


        Assert.That(
            () => editorHotspotViewModel.AddMedia((MediaEditorType)2, Array.Empty<IStorageFile>()),
            Throws.InstanceOf<ArgumentOutOfRangeException>()
        );
    }

    [AvaloniaTest]
    public async Task RemoveMediaTest()
    {
        var hotspot = CreateHotspot();
        var editorHotspotViewModel = new EditorHotspotViewModel(hotspot, new MockViewModelProvider());
        editorHotspotViewModel.AddMedia(MediaEditorType.Images, new[] { await GetFile("test_image.png") });

        var image = editorHotspotViewModel.Images[0];
        var video = editorHotspotViewModel.Videos[0];

        editorHotspotViewModel.RemoveMedia(MediaEditorType.Images, new[] { image });
        editorHotspotViewModel.RemoveMedia(MediaEditorType.Videos, new[] { video });

        Assert.Multiple(() =>
        {
            Assert.That(editorHotspotViewModel.Images, Has.Exactly(2).Items);
            Assert.That(editorHotspotViewModel.Videos, Is.Empty);
        });
        Assert.That(editorHotspotViewModel.Images[0].FilePath, Is.EqualTo(hotspot.ImagePaths[1]));

        editorHotspotViewModel.RemoveMedia(MediaEditorType.Images, editorHotspotViewModel.Images);

        Assert.That(editorHotspotViewModel.Images, Is.Empty);


        Assert.That(
            () => editorHotspotViewModel.RemoveMedia((MediaEditorType)2, Array.Empty<IThumbnailViewModel>()),
            Throws.InstanceOf<ArgumentOutOfRangeException>()
        );
    }

    [AvaloniaTest]
    public async Task ToHotspotTest()
    {
        var hotspot = CreateHotspot();
        var editorHotspotViewModel = new EditorHotspotViewModel(hotspot, new MockViewModelProvider());

        var image = await GetFile("test_image.png");
        var video = await GetFile("test_video.mp4");

        editorHotspotViewModel.AddMedia(MediaEditorType.Images, new[] { image });
        editorHotspotViewModel.AddMedia(MediaEditorType.Videos, new[] { video });

        var newHotspot = editorHotspotViewModel.ToHotspot();

        Assert.Multiple(() =>
        {
            Assert.That(newHotspot.Id, Is.EqualTo(hotspot.Id));
            Assert.That(newHotspot.Position, Is.EqualTo(hotspot.Position));
            Assert.That(newHotspot.Title, Is.EqualTo(hotspot.Title));
            Assert.That(newHotspot.DescriptionPath, Is.Not.EqualTo(hotspot.DescriptionPath));
            Assert.That(newHotspot.ImagePaths, Is.EquivalentTo(hotspot.ImagePaths.Append(image.Path.AbsolutePath)));
            Assert.That(newHotspot.VideoPaths, Is.EquivalentTo(hotspot.VideoPaths.Append(video.Path.AbsolutePath)));
        });
    }
}
