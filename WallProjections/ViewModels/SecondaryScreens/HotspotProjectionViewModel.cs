using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;

namespace WallProjections.ViewModels.SecondaryScreens;

/// <inheritdoc cref="AbsHotspotProjectionViewModel" />
public class HotspotProjectionViewModel : AbsHotspotProjectionViewModel
{
    /// <inheritdoc />
    public override int Id { get; }

    /// <summary>
    /// The X coordinate of the top-left corner of the hotspot in pixels
    /// </summary>
    /// <remarks>
    /// Because the hotspot is a circle, this is the X coordinate of the top-left corner of the bounding box
    /// </remarks>
    public override double X { get; }

    /// <summary>
    /// The Y coordinate of the top-left corner of the hotspot in pixels
    /// </summary>
    /// <remarks>
    /// Because the hotspot is a circle, this is the Y coordinate of the top-left corner of the bounding box
    /// </remarks>
    public override double Y { get; }

    /// <inheritdoc />
    public override double D { get; }

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
