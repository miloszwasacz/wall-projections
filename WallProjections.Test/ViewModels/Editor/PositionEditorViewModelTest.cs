using WallProjections.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.ViewModels.Editor;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Test.ViewModels.Editor;

[TestFixture]
public class PositionEditorViewModelTest
{
    private static IPositionEditorViewModel SetupPositionEditorViewModel()
    {
        IPositionEditorViewModel positionEditor = new PositionEditorViewModel();
        positionEditor.SelectHotspot(new EditorHotspotViewModel(0, new MockViewModelProvider()), Enumerable.Empty<Coord>());
        return positionEditor;
    }
    
    [AvaloniaTest]
    public void SetPositionTest()
    {
        var positionEditor = SetupPositionEditorViewModel();
        Assert.Multiple(() =>
        {
            Assert.That(positionEditor.X, Is.EqualTo(0));
            Assert.That(positionEditor.Y, Is.EqualTo(0));
        });
        
        positionEditor.SetPosition(50, 40);
        Assert.Multiple(() =>
        {
            Assert.That(positionEditor.X, Is.EqualTo(0));
            Assert.That(positionEditor.Y, Is.EqualTo(0));
        });
        
        positionEditor.IsInEditMode = true;
        positionEditor.SetPosition(50, 40);
        Assert.Multiple(() =>
        {
            Assert.That(positionEditor.X, Is.EqualTo(50));
            Assert.That(positionEditor.Y, Is.EqualTo(40));
        });
    }

    [AvaloniaTest]
    public void ChangeRadiusTest()
    {
        var positionEditor = SetupPositionEditorViewModel();
        Assert.That(positionEditor.D, Is.EqualTo(60));
        positionEditor.ChangeRadius(20);
        Assert.That(positionEditor.D, Is.EqualTo(60));
        positionEditor.IsInEditMode = true;
        positionEditor.ChangeRadius(20);
        Assert.That(positionEditor.D, Is.EqualTo(100));
        positionEditor.ChangeRadius(-40);
        Assert.That(positionEditor.D, Is.EqualTo(20));
        positionEditor.ChangeRadius(-50);
        Assert.That(positionEditor.D, Is.EqualTo(0));
    }

    [AvaloniaTest]
    public void UpdateSelectedHotspotTest()
    {
        var hotspot = new EditorHotspotViewModel(0, new MockViewModelProvider());
        var newHotspot = new EditorHotspotViewModel(1, new MockViewModelProvider());
        IEditorHotspotViewModel[] hotspots = { hotspot, newHotspot };
        
        var positionEditor = SetupPositionEditorViewModel();
        
        positionEditor.IsInEditMode = true;
        positionEditor.SetPosition(40,50);
        positionEditor.ChangeRadius(20);
        positionEditor.IsInEditMode = false;
        positionEditor.UpdateSelectedHotspot();
        positionEditor.SelectHotspot(newHotspot, hotspots.Where(h => h != newHotspot).Select(h => h.Position));
        
        Assert.Multiple(() =>
        {
            Assert.That(positionEditor.UnselectedHotspots.First().X, Is.EqualTo(0));
            Assert.That(positionEditor.UnselectedHotspots.First().Y, Is.EqualTo(0));
            Assert.That(positionEditor.UnselectedHotspots.First().D, Is.EqualTo(60));
        });
        positionEditor.SelectHotspot(hotspot, hotspots.Where(h => h != hotspot).Select(h => h.Position));
        positionEditor.IsInEditMode = true;
        positionEditor.SetPosition(40,50);
        positionEditor.ChangeRadius(20);
        positionEditor.UpdateSelectedHotspot();
        positionEditor.SelectHotspot(newHotspot, hotspots.Where(h => h != newHotspot).Select(h => h.Position));
        Assert.Multiple(() =>
        {
            Assert.That(positionEditor.UnselectedHotspots.First().X, Is.EqualTo(40));
            Assert.That(positionEditor.UnselectedHotspots.First().Y, Is.EqualTo(50));
            Assert.That(positionEditor.UnselectedHotspots.First().D, Is.EqualTo(100));
        });
        
    }

    [AvaloniaTest]
    public void IsInEditModeTest()
    {   //testing changing IsInEditMode when _selectedHotspot is null
        IPositionEditorViewModel positionEditor = new PositionEditorViewModel();
        positionEditor.SetPosition(30,30);
        Assert.Multiple(() =>
        {
            Assert.That(positionEditor.X, Is.EqualTo(0));
            Assert.That(positionEditor.Y, Is.EqualTo(0));
        });
        positionEditor.IsInEditMode = true;
        positionEditor.SetPosition(30,30);
        Assert.Multiple(() =>
        {
            Assert.That(positionEditor.X, Is.EqualTo(0));
            Assert.That(positionEditor.Y, Is.EqualTo(0));
        });
        
        //testing changing IsInEditMode when _selectedHotspot is not null
        positionEditor = SetupPositionEditorViewModel();
        positionEditor.IsInEditMode = true;
        positionEditor.SetPosition(30,30);
        Assert.Multiple(() =>
        {
            Assert.That(positionEditor.X, Is.EqualTo(30));
            Assert.That(positionEditor.Y, Is.EqualTo(30));
        });
    }
    
}
