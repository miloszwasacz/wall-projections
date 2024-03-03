using ReactiveUI;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;

namespace WallProjections.ViewModels.SecondaryScreens;

/// <inheritdoc cref="IHotspotProjectionViewModel" />
public class HotspotProjectionViewModel : ViewModelBase, IHotspotProjectionViewModel
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
    /// The number of pixels from the leftmost side
    /// </summary>
    public double X { get; }

    /// <summary>
    /// The number of pixels from the top
    /// </summary>
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
        X = hotspot.Position.X;
        Y = hotspot.Position.Y;
        D = hotspot.Position.R * 2;
    }
}
