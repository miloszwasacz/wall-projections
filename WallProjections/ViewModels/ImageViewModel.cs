using System;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.ViewModels;

public class ImageViewModel : ViewModelBase, IImageViewModel
{
    private Bitmap? _imageToLoad;
    
    private static readonly Uri FallbackImagePath = new("avares://WallProjections/Assets/fallback.png"); 
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

    public bool ShowImage(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Image = new Bitmap(AssetLoader.Open(FallbackImagePath));
            return false;
        }
        using var fileStream = File.OpenRead(filePath);
        Image = new Bitmap(fileStream);
        return true;
    }

    public void HideImage()
    {
        Image = null;
    }
}
