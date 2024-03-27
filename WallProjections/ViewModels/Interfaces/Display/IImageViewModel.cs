using Avalonia.Media.Imaging;

namespace WallProjections.ViewModels.Interfaces.Display;

public interface IImageViewModel
{
    /// <summary>
    /// The image to be displayed.
    /// </summary>
    public Bitmap? Image { get; }

    /// <summary>
    /// Whether or not the viewmodel has an image to display.
    /// </summary>
    public bool HasImages { get; }

    /// <summary>
    /// Shows the image at the given path.
    /// </summary>
    /// <param name="filePath">The path to the image.</param>
    /// <returns>Whether or not the image was successfully shown.</returns>
    public bool ShowImage(string filePath);

    /// <summary>
    /// Hides the currently displayed image.
    /// </summary>
    public void HideImage();
}
