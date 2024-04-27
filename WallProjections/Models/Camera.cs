namespace WallProjections.Models;

/// <summary>
/// A camera used by OpenCV.
/// </summary>
/// <param name="Index">The index passed to OpenCV.</param>
/// <param name="Name">The device name.</param>
public record struct Camera(int Index, string Name)
{
    public string DisplayName => $"{Name} (index: {Index})";
}
