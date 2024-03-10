using WallProjections.Models;
using WallProjections.ViewModels.Editor;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Test.Mocks.ViewModels.Editor;

public class MockPositionEditorViewModel : IPositionEditorViewModel
{
    /// <inheritdoc/>
    public event EventHandler? HotspotPositionChanged;
    
    /// <inheritdoc/>
    public bool IsInEditMode { get; set; }
    
    /// <inheritdoc/>
    public IEditorHotspotViewModel? SelectedHotspot { get; set; }
    
    /// <inheritdoc/>
    public IEnumerable<Coord> UnselectedHotspots { get; set; } = Enumerable.Empty<Coord>();
    
    /// <inheritdoc/>
    public double X
    {
        get;
        private set;
    }

    /// <inheritdoc/>
    public double Y
    {
        get;
        private set;
    }

    /// <inheritdoc/>
    public double R
    {
        get;
        private set;
    }

    /// <summary>
    /// Mock version of the SetPosition function in <see cref="PositionEditorViewModel"/>
    /// which sets X to <paramref name="x"/> and
    /// Y to <paramref name="y"/>
    /// </summary>
    /// <param name="x">Changes the X coordinate</param>
    /// <param name="y">Changes the Y coordinate</param>
    public void SetPosition(double x, double y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Mock version of the ChangeRadius function in <see cref="PositionEditorViewModel"/>
    /// which adds double <paramref name="delta"/> to D
    /// </summary>
    /// <param name="delta">The increase in radius</param>
    public void ChangeRadius(double delta)
    {
        R += delta;
    }

    /// <summary>
    /// Mock version of the UpdateSelectedHotspot function in <see cref="PositionEditorViewModel"/>
    /// which creates a new <see cref="MockEditorHotspotViewModel"/> with id 1,
    /// position with the X, Y and half of D provided, along with the
    /// title "name" and description "description
    /// </summary>
    public void UpdateSelectedHotspot()
    {
        SelectedHotspot = new MockEditorHotspotViewModel(
            1, 
            new Coord(X, Y, R),
            "name", 
            "description");
    }
}
