using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Test.Mocks.ViewModels.Editor;

public class MockPositionEditorViewModel : IPositionEditorViewModel
{
    public event EventHandler? HotspotPositionChanged;
    public bool IsInEditMode { get; set; }
    public IEditorHotspotViewModel? SelectedHotspot { get; set; }
    public IEnumerable<ViewCoord> UnselectedHotspots { get; set; } = Enumerable.Empty<ViewCoord>();

    public double X
    {
        get;
        private set;
    }

    public double Y
    {
        get;
        private set;
    }

    public double D
    {
        get;
        private set;
    }

    public void SetPosition(double x, double y)
    {
        X = x;
        Y = y;
    }

    public void ChangeRadius(double delta)
    {
        D += delta * 2;
    }

    public void UpdateSelectedHotspot()
    {
        SelectedHotspot = new MockEditorHotspotViewModel(
            1, 
            new Coord(X, Y, D / 2),
            "name", 
            "description");
    }
}
