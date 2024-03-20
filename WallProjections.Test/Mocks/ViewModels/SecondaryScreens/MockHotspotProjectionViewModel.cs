using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;

namespace WallProjections.Test.Mocks.ViewModels.SecondaryScreens;

public class MockHotspotProjectionViewModel : IHotspotProjectionViewModel
{
    public int Id { get; }
    public double X { get; }
    public double Y { get; }
    public double D { get; }
    public bool IsActivating { get; set; }
    public bool IsActive { get; set; }

    public MockHotspotProjectionViewModel(Hotspot hotspot)
    {
        Id = hotspot.Id;
        X = hotspot.Position.X - hotspot.Position.R;
        Y = hotspot.Position.Y - hotspot.Position.R;
        D = 2 * hotspot.Position.R;
    }
}

public static class MockHotspotProjectionViewModelExtensions
{
    /// <summary>
    /// The tolerance for comparing floating point numbers (i.e. hotspot positions)
    /// </summary>
    private const double PositionCmpTolerance = 0.001;

    /// <summary>
    /// Checks if the viewmodel has the same ID, X, Y and D as the hotspot
    /// </summary>
    public static bool IsSameAsHotspot(this IHotspotProjectionViewModel self, Hotspot hotspot)
    {
        var id = self.Id == hotspot.Id;
        var x = Math.Abs(self.X - (hotspot.Position.X - hotspot.Position.R)) < PositionCmpTolerance;
        var y = Math.Abs(self.Y - (hotspot.Position.Y - hotspot.Position.R)) < PositionCmpTolerance;
        var d = Math.Abs(self.D - 2 * hotspot.Position.R) < PositionCmpTolerance;
        return id && x && y && d;
    }
}
