using Avalonia.Media.Imaging;

namespace WallProjections.ViewModels.Interfaces;

public interface IImageViewModel
{
    public Bitmap? Image { get; }
    public bool HasImages { get; }
    public bool ShowImage(string filePath);
    public void HideImage();
}
