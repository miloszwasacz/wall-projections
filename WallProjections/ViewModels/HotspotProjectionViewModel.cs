using ReactiveUI;
using WallProjections.Models;
using WallProjections.Models.Interfaces;

namespace WallProjections.ViewModels;

/// <summary>
/// A record of all the parameters required to display a hotspot
/// </summary>
public class HotspotProjectionViewModel : ViewModelBase, IPosition
{
    /// <summary>
    /// The backing field for <see cref="IsActive" />
    /// </summary>
    private bool _isActive;

    /// <summary>
    /// The id of the hotspot to be activated
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// The X coordinate of the top-left corner of the hotspot in pixels
    /// </summary>
    /// <remarks>
    /// Because the hotspot is a circle, this is the X coordinate of the top-left corner of the bounding box
    /// </remarks>
    public double X { get; }

    /// <summary>
    /// The Y coordinate of the top-left corner of the hotspot in pixels
    /// </summary>
    /// <remarks>
    /// Because the hotspot is a circle, this is the Y coordinate of the top-left corner of the bounding box
    /// </remarks>
    public double Y { get; }

    /// <summary>
    /// The diameter of the hotspot
    /// </summary>
    public double D { get; }

    /// <summary>
    /// Shows whether the hotspot is activated or not
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    /// <summary>
    /// Creates a new <see cref="HotspotProjectionViewModel" /> based on the provided <paramref name="hotspot" />
    /// </summary>
    /// <param name="hotspot">The hotspot to be projected</param>
    public HotspotProjectionViewModel(Hotspot hotspot)
    {
        Id = hotspot.Id;
        X = hotspot.Position.X - hotspot.Position.R;
        Y = hotspot.Position.Y - hotspot.Position.R;
        D = hotspot.Position.R * 2;
    }
}
