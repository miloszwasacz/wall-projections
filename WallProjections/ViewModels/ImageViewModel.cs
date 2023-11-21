using System.IO;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace WallProjections.ViewModels;

public class ImageViewModel : ViewModelBase
{
    private Bitmap? _imageToLoad;

    public Bitmap? Image
    {
        get => _imageToLoad;
        private set
        {
            this.RaiseAndSetIfChanged(ref _imageToLoad, value);
            this.RaisePropertyChanged(nameof(HasImages));
        }
    }

    public bool HasImages => Image is not null;

    public void ShowImage(string filePath)
    {
        using var fileStream = File.OpenRead(filePath);
        Image = new Bitmap(fileStream);
    }

    public void HideImage()
    {
        Image = null;
    }
}
