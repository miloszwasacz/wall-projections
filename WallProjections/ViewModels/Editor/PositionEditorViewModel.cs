using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ReactiveUI;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.ViewModels.Editor;

/// <inheritdoc cref="IPositionEditorViewModel" />
public class PositionEditorViewModel : ViewModelBase, IPositionEditorViewModel
{
    /// <inheritdoc />
    public event EventHandler? HotspotPositionChanged;

    /// <summary>
    /// A mutex ensuring sequential access to the position and radius of the hotspot.
    /// </summary>
    private readonly Mutex _mutex = new();

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
    private IEnumerable<ViewCoord> _unselectedHotspots = Enumerable.Empty<ViewCoord>();

    /// <inheritdoc />
    public bool IsInEditMode
    {
        get => _isInEditMode;
        set
        {
            if (_selectedHotspot is null) return;

            _isInEditMode = value;

            // Reset the position and radius if the user cancels the edit
            SelectedHotspot = _selectedHotspot;
        }
    }

    /// <inheritdoc />
    public IEditorHotspotViewModel? SelectedHotspot
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
    public IEnumerable<ViewCoord> UnselectedHotspots
    {
        get => _unselectedHotspots;
        set => this.RaiseAndSetIfChanged(ref _unselectedHotspots, value);
    }

    /// <inheritdoc />
    public double X
    {
        get => _x;
        private set => this.RaiseAndSetIfChanged(ref _x, value);
    }

    /// <inheritdoc />
    public double Y
    {
        get => _y;
        private set => this.RaiseAndSetIfChanged(ref _y, value);
    }
    
    /// <summary>
    /// The current radius of the hotspot (non-negative). Notifies <see cref="D" /> about changes.
    /// </summary>
    /// <remarks>
    /// This is not the same as <see cref="SelectedHotspot" />.<see cref="IEditorHotspotViewModel.Position" />.<see cref="Coord.R" />!
    /// To update the position of the selected hotspot, call <see cref="UpdateSelectedHotspot" />.
    /// </remarks>
    private double R
    {
        get => _r;
        set
        {
            this.RaiseAndSetIfChanged(ref _r, Math.Max(value, 0));
            this.RaisePropertyChanged(nameof(D));
        }
    }

    /// <inheritdoc />
    public double D => _r * 2;

    /// <inheritdoc />
    public void SetPosition(double x, double y)
    {
        if (!IsInEditMode || _selectedHotspot is null) return;

        _mutex.WaitOne();
        X = x;
        Y = y;
        _mutex.ReleaseMutex();
    }

    /// <inheritdoc />
    public void ChangeRadius(double delta)
    {
        if (!IsInEditMode || _selectedHotspot is null) return;

        _mutex.WaitOne();
        R += delta;
        _mutex.ReleaseMutex();
    }

    /// <inheritdoc />
    public void UpdateSelectedHotspot()
    {
        if (!IsInEditMode || _selectedHotspot is null) return;

        _mutex.WaitOne();
        _selectedHotspot.Position = new Coord(X, Y, R);
        _mutex.ReleaseMutex();

        IsInEditMode = false;
        HotspotPositionChanged?.Invoke(this, EventArgs.Empty);
    }
}
