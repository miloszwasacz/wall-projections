using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Test.Mocks.ViewModels.Editor;

public class MockPositionEditorViewModel : IPositionEditorViewModel
{
    public bool IsInEditMode { get; set; }
    public IEditorHotspotViewModel? SelectedHotspot { get; set; }
    public IEnumerable<Coord> UnselectedHotspots { get; set; }
    public double X { get; }
    public double Y { get; }
    public double R { get; }

    public void SetPosition(double x, double y)
    {
        throw new NotImplementedException();
    }

    public void ChangeRadius(double delta)
    {
        throw new NotImplementedException();
    }

    public void UpdateSelectedHotspot()
    {
        throw new NotImplementedException();
    }
}
