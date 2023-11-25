using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.Test.Mocks.ViewModels;

/// <summary>
/// A mock of <see cref="ImageViewModel" /> for injecting into <see cref="DisplayViewModel" />
/// </summary>
public class MockImageViewModel : IImageViewModel
{
    /// <summary>
    /// The backing field for <see cref="ImagePaths" />
    /// </summary>
    private readonly List<string> _imagePaths = new();

    /// <summary>
    /// A list of paths to the images the viewmodel has shown
    /// </summary>
    public IReadOnlyList<string> ImagePaths => _imagePaths;

    /// <summary>
    /// The number of times <see cref="ShowImage" /> has been called
    /// </summary>
    public int ShowCount => _imagePaths.Count;

    /// <summary>
    /// The number of times <see cref="HideImage" /> has been called
    /// </summary>
    public int HideCount { get; private set; }

    public Bitmap? Image => _imagePaths.LastOrDefault() is not null
        ? new Bitmap(PixelFormats.Gray2, AlphaFormat.Opaque, IntPtr.Zero, PixelSize.Empty, Vector.Zero, 0)
        : null;

    /// <inheritdoc />
    public bool HasImages { get; private set; }

    /// <summary>
    /// Increases the number of times <see cref="ShowImage" /> has been called
    /// and adds <paramref name="filePath" /> to <see cref="ImagePaths" />
    /// </summary>
    public void ShowImage(string filePath)
    {
        _imagePaths.Add(filePath);
        HasImages = true;
    }

    /// <summary>
    /// Increases the number of times <see cref="HideImage" /> has been called
    /// </summary>
    public void HideImage()
    {
        HideCount++;
        HasImages = false;
    }
}
