using System.IO;
using Avalonia.Media.Imaging;
using ReactiveUI;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.ViewModels;

public class ImageViewModel : ViewModelBase, IImageViewModel
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
        if (File.Exists(filePath))
        {
            using var fileStream = File.OpenRead(filePath);
            Image = new Bitmap(fileStream);
        }
        else
        {
            Image = null;
        }
    }

    public void HideImage()
    {
        Image = null;
    }
}
