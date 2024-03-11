using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.ViewModels.SecondaryScreens;

namespace WallProjections.Test.Mocks.ViewModels.Editor;

public class MockPositionEditorViewModel : IPositionEditorViewModel
{
    /// <summary>
    /// The backing field for <see cref="SelectedHotspot" />
    /// </summary>
    private IEditorHotspotViewModel? _selectedHotspot;

    /// <inheritdoc/>
    public override event EventHandler? HotspotPositionChanged;

    /// <inheritdoc/>
    public override bool IsInEditMode { get; set; }

    /// <inheritdoc cref="SelectedHotspot" />
    public IEditorHotspotViewModel? GetSelectedHotspot() => _selectedHotspot;

    /// <inheritdoc/>
    protected override IEditorHotspotViewModel? SelectedHotspot
    {
        set => _selectedHotspot = value;
    }

    /// <inheritdoc/>
    public override IEnumerable<Coord> UnselectedHotspots { get; protected set; } = Enumerable.Empty<Coord>();

    /// <inheritdoc/>
    public override double X { get; protected set; }

    /// <inheritdoc/>
    public override double Y { get; protected set; }

    /// <inheritdoc/>
    public override double R { get; protected set; }

    /// <summary>
    /// Mock version of the SetPosition function in <see cref="PositionEditorViewModel"/>
    /// which sets X to <paramref name="x"/> and
    /// Y to <paramref name="y"/>
    /// </summary>
    /// <param name="x">Changes the X coordinate</param>
    /// <param name="y">Changes the Y coordinate</param>
    public override void SetPosition(double x, double y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Mock version of the ChangeRadius function in <see cref="PositionEditorViewModel"/>
    /// which adds double <paramref name="delta"/> to D
    /// </summary>
    /// <param name="delta">The increase in radius</param>
    public override void ChangeRadius(double delta)
    {
        R += delta;
    }

    /// <summary>
    /// Mock version of the UpdateSelectedHotspot function in <see cref="PositionEditorViewModel"/>
    /// which creates a new <see cref="MockEditorHotspotViewModel"/> with id 1,
    /// position with the X, Y and half of D provided, along with the
    /// title "name" and description "description
    /// </summary>
    public override void UpdateSelectedHotspot()
    {
        SelectedHotspot = new MockEditorHotspotViewModel(
            1,
            new Coord(X, Y, R),
            "name",
            "description");
    }
}
