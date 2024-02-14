using WallProjections.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.ViewModels.Editor;
using WallProjections.ViewModels.Editor;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Test.ViewModels.Editor;

[TestFixture]
public class DescriptionEditorViewModelTest
{
    // ReSharper disable once InconsistentNaming
    private static readonly MockViewModelProvider VMProvider = new();

    private static MockEditorHotspotViewModel CreateHotspot(int id = 0) =>
        new(id, new Coord(0, 0, 0), $"Title {id}", $"Description {id}");

    [Test]
    public void ConstructorTest()
    {
        var descriptionEditorViewModel = new DescriptionEditorViewModel(VMProvider);

        Assert.Multiple(() =>
        {
            Assert.That(descriptionEditorViewModel.Title, Is.Empty);
            Assert.That(descriptionEditorViewModel.Description, Is.Empty);
            Assert.That(descriptionEditorViewModel.IsEnabled, Is.False);
            Assert.That(descriptionEditorViewModel.Importer, Is.InstanceOf<IImportViewModel>());
        });
    }

    [Test]
    public void HotspotTest()
    {
        var hotspot = CreateHotspot();
        var changed = false;

        var descriptionEditorViewModel = new DescriptionEditorViewModel(VMProvider);
        descriptionEditorViewModel.ContentChanged += (_, _) => changed = true;

        Assert.Multiple(() =>
        {
            Assert.That(descriptionEditorViewModel.Title, Is.Empty);
            Assert.That(descriptionEditorViewModel.Description, Is.Empty);
            Assert.That(descriptionEditorViewModel.IsEnabled, Is.False);
            Assert.That(changed, Is.False);
        });

        descriptionEditorViewModel.Hotspot = hotspot;

        Assert.Multiple(() =>
        {
            Assert.That(descriptionEditorViewModel.Title, Is.EqualTo(hotspot.Title));
            Assert.That(descriptionEditorViewModel.Description, Is.EqualTo(hotspot.Description));
            Assert.That(descriptionEditorViewModel.IsEnabled, Is.True);
            Assert.That(changed, Is.False);
        });
    }

    [Test]
    public void TitleTest()
    {
        const string title = "Test";
        var hotspot = CreateHotspot();
        var changed = false;

        var descriptionEditorViewModel = new DescriptionEditorViewModel(VMProvider)
        {
            Hotspot = hotspot
        };
        descriptionEditorViewModel.ContentChanged += (_, _) => changed = true;

        Assert.Multiple(() =>
        {
            Assert.That(descriptionEditorViewModel.Title, Is.EqualTo(hotspot.Title));
            Assert.That(descriptionEditorViewModel.Description, Is.EqualTo(hotspot.Description));
            Assert.That(descriptionEditorViewModel.IsEnabled, Is.True);
            Assert.That(changed, Is.False);
        });

        descriptionEditorViewModel.Title = title;

        Assert.Multiple(() =>
        {
            Assert.That(hotspot.Title, Is.EqualTo(title));
            Assert.That(descriptionEditorViewModel.Title, Is.EqualTo(title));
            Assert.That(descriptionEditorViewModel.Description, Is.EqualTo(hotspot.Description));
            Assert.That(descriptionEditorViewModel.IsEnabled, Is.True);
            Assert.That(changed, Is.True);
        });
    }


    [Test]
    public void TitleNoHotspotTest()
    {
        const string title = "Test";
        var changed = false;

        var descriptionEditorViewModel = new DescriptionEditorViewModel(VMProvider);
        descriptionEditorViewModel.ContentChanged += (_, _) => changed = true;

        Assert.Multiple(() =>
        {
            Assert.That(descriptionEditorViewModel.Title, Is.Empty);
            Assert.That(descriptionEditorViewModel.Description, Is.Empty);
            Assert.That(descriptionEditorViewModel.IsEnabled, Is.False);
            Assert.That(changed, Is.False);
        });

        descriptionEditorViewModel.Title = title;

        Assert.Multiple(() =>
        {
            Assert.That(descriptionEditorViewModel.Title, Is.Empty);
            Assert.That(descriptionEditorViewModel.Description, Is.Empty);
            Assert.That(descriptionEditorViewModel.IsEnabled, Is.False);
            Assert.That(changed, Is.False);
        });
    }

    [Test]
    public void DescriptionTest()
    {
        const string description = "Test";
        var hotspot = CreateHotspot();
        var changed = false;

        var descriptionEditorViewModel = new DescriptionEditorViewModel(VMProvider)
        {
            Hotspot = hotspot
        };
        descriptionEditorViewModel.ContentChanged += (_, _) => changed = true;

        Assert.Multiple(() =>
        {
            Assert.That(descriptionEditorViewModel.Title, Is.EqualTo(hotspot.Title));
            Assert.That(descriptionEditorViewModel.Description, Is.EqualTo(hotspot.Description));
            Assert.That(descriptionEditorViewModel.IsEnabled, Is.True);
            Assert.That(changed, Is.False);
        });

        descriptionEditorViewModel.Description = description;

        Assert.Multiple(() =>
        {
            Assert.That(hotspot.Description, Is.EqualTo(description));
            Assert.That(descriptionEditorViewModel.Title, Is.EqualTo(hotspot.Title));
            Assert.That(descriptionEditorViewModel.Description, Is.EqualTo(description));
            Assert.That(descriptionEditorViewModel.IsEnabled, Is.True);
            Assert.That(changed, Is.True);
        });
    }

    [Test]
    public void DescriptionNoHotspotTest()
    {
        const string description = "Test";
        var descriptionEditorViewModel = new DescriptionEditorViewModel(VMProvider);
        var changed = false;
        descriptionEditorViewModel.ContentChanged += (_, _) => changed = true;

        Assert.Multiple(() =>
        {
            Assert.That(descriptionEditorViewModel.Title, Is.Empty);
            Assert.That(descriptionEditorViewModel.Description, Is.Empty);
            Assert.That(descriptionEditorViewModel.IsEnabled, Is.False);
            Assert.That(changed, Is.False);
        });

        descriptionEditorViewModel.Description = description;

        Assert.Multiple(() =>
        {
            Assert.That(descriptionEditorViewModel.Title, Is.Empty);
            Assert.That(descriptionEditorViewModel.Description, Is.Empty);
            Assert.That(descriptionEditorViewModel.IsEnabled, Is.False);
            Assert.That(changed, Is.False);
        });
    }
}
