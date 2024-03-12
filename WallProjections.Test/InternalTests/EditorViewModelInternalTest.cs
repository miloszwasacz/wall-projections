using System.Reflection;
using WallProjections.Models;
using WallProjections.Test.Mocks.Helper;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.ViewModels.Editor;
using WallProjections.ViewModels.Editor;

// ReSharper disable AccessToStaticMemberViaDerivedType
using Is = WallProjections.Test.ViewModels.Editor.EditorViewModelTestExtensions.Is;

namespace WallProjections.Test.InternalTests;

[TestFixture]
public class EditorViewModelInternalTest
{
    [Test]
    public void SkipSelectedHotspotUpdateOnItemChangeTest()
    {
        var navigator = new MockNavigator();
        var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();

        var editorViewModel = new EditorViewModel(navigator, fileHandler, pythonHandler, vmProvider);

        editorViewModel.AddHotspot();
        editorViewModel.AddHotspot();
        Assert.That(editorViewModel.Hotspots, Has.Count.EqualTo(2));

        var selected = editorViewModel.Hotspots[1];
        Assert.That(editorViewModel.SelectedHotspot, Is.SameAs(selected));

        // Simulate a property change of some item in the collection
        var itemUpdating = editorViewModel.Hotspots.GetType()
            .GetProperty("IsItemUpdating", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        itemUpdating!.SetValue(editorViewModel.Hotspots, true);

        editorViewModel.SelectedHotspot = editorViewModel.Hotspots[0];

        Assert.Multiple(() =>
        {
            Assert.That(editorViewModel.SelectedHotspot, Is.SameAs(selected));
            Assert.That(
                (editorViewModel.DescriptionEditor as MockDescriptionEditorViewModel)!.Hotspot,
                Is.SameAs(selected)
            );
            Assert.That(editorViewModel.ImageEditor.Media, Is.EquivalentTo(selected.Images));
            Assert.That(editorViewModel.VideoEditor.Media, Is.EquivalentTo(selected.Videos));
            Assert.That(editorViewModel, Is.Unsaved);
        });
    }
}
