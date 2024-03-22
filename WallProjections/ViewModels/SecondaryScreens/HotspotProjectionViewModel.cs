using ReactiveUI;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;

namespace WallProjections.ViewModels.SecondaryScreens;

/// <inheritdoc cref="IHotspotProjectionViewModel" />
public class HotspotProjectionViewModel : ViewModelBase, IHotspotProjectionViewModel
{
    /// <summary>
    /// The backing field for <see cref="IsActivating" />
    /// </summary>
    private bool _isActivating;

    /// <summary>
    /// The backing field for <see cref="IsDeactivating" />
    /// </summary>
    private bool _isDeactivating;

    /// <summary>
    /// The backing field for <see cref="IsActive" />
    /// </summary>
    private bool _isActive;

    /// <inheritdoc />
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

    /// <inheritdoc />
    public double D { get; }

    /// <inheritdoc />
    public bool IsActivating
    {
        get => _isActivating;
        set => this.RaiseAndSetIfChanged(ref _isActivating, value);
    }

    /// <inheritdoc />
    public bool IsDeactivating
    {
        get => _isDeactivating;
        set => this.RaiseAndSetIfChanged(ref _isDeactivating, value);
    }

    /// <inheritdoc />
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
