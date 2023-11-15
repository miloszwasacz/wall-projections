using System.Diagnostics;
using System.IO;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace WallProjections.ViewModels;

public class ImageViewModel : ViewModelBase
{
    private string ImagePath { get; }
    private Bitmap? _imageToLoad;
    public Bitmap? ImageToLoadPublic
    {
        get => _imageToLoad;
        private set => this.RaiseAndSetIfChanged(ref _imageToLoad, value);
    }

    public ImageViewModel(string filePath)
    {
        ImagePath = filePath;
        Debug.Print(ImagePath);
        ImageToLoadPublic = ChangeImage(ImagePath);
    }

    private Bitmap ChangeImage(string filePath)
    {
        using var fileStream = File.OpenRead(filePath);
        return new Bitmap(fileStream);
    }
}
