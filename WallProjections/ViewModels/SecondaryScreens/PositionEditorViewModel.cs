using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;

namespace WallProjections.ViewModels.SecondaryScreens;

/// <inheritdoc cref="AbsPositionEditorViewModel" />
public class PositionEditorViewModel : AbsPositionEditorViewModel
{
    /// <inheritdoc />
    public override event EventHandler? HotspotPositionChanged;

    /// <summary>
    /// The backing field for <see cref="IsInEditMode" />.
    /// </summary>
    private bool _isInEditMode;

    /// <summary>
    /// The backing field for <see cref="SelectedHotspot" />.
    /// </summary>
    private IEditorHotspotViewModel? _selectedHotspot;

    /// <summary>
    /// The backing field for <see cref="X" />.
    /// </summary>
    private double _x;

    /// <summary>
    /// The backing field for <see cref="Y" />.
    /// </summary>
    private double _y;

    /// <summary>
    /// The backing field for <see cref="R" />.
    /// </summary>
    private double _r;

    /// <summary>
    /// The backing field for <see cref="UnselectedHotspots" />.
    /// </summary>
    private IEnumerable<Coord> _unselectedHotspots = Enumerable.Empty<Coord>();

    /// <inheritdoc />
    public override bool IsInEditMode
    {
        get => _isInEditMode;
        set
        {
            if (_selectedHotspot is null) return;

            this.RaiseAndSetIfChanged(ref _isInEditMode, value);

            // Reset the position and radius if the user cancels the edit
            SelectedHotspot = _selectedHotspot;
        }
    }

    /// <inheritdoc />
    protected override IEditorHotspotViewModel? SelectedHotspot
    {
        set
        {
            //assigns the new values
            _selectedHotspot = value;
            X = _selectedHotspot?.Position.X ?? 0;
            Y = _selectedHotspot?.Position.Y ?? 0;
            R = _selectedHotspot?.Position.R ?? 0;
        }
    }

    /// <inheritdoc />
    public override IEnumerable<Coord> UnselectedHotspots
    {
        get => _unselectedHotspots;
        protected set => this.RaiseAndSetIfChanged(ref _unselectedHotspots, value);
    }

    /// <inheritdoc />
    public override double X
    {
        get => _x;
        protected set => this.RaiseAndSetIfChanged(ref _x, value);
    }

    /// <inheritdoc />
    public override double Y
    {
        get => _y;
        protected set => this.RaiseAndSetIfChanged(ref _y, value);
    }

    /// <inheritdoc />
    public override double R
    {
        get => _r;
        protected set => this.RaiseAndSetIfChanged(ref _r, Math.Max(value, 0));
    }

    /// <inheritdoc />
    public override void SetPosition(double x, double y)
    {
        if (!IsInEditMode || _selectedHotspot is null) return;

        lock (this)
        {
            X = x;
            Y = y;
        }
    }

    /// <inheritdoc />
    public override void ChangeRadius(double delta)
    {
        if (!IsInEditMode || _selectedHotspot is null) return;

        lock (this)
        {
            R += delta;
        }
    }

    /// <inheritdoc />
    public override void UpdateSelectedHotspot()
    {
        if (!IsInEditMode || _selectedHotspot is null) return;

        lock (this)
        {
            _selectedHotspot.Position = new Coord(X, Y, R);
        }

        IsInEditMode = false;
        HotspotPositionChanged?.Invoke(this, EventArgs.Empty);
    }
}
