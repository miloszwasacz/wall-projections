using System;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;
using WallProjections.ViewModels.Interfaces.Display;

namespace WallProjections.ViewModels.Display;

/// <inheritdoc cref="IImageViewModel" />
public class ImageViewModel : ViewModelBase, IImageViewModel
{
    /// <summary>
    /// The backing field for <see cref="Image" />
    /// </summary>
    private Bitmap? _imageToLoad;

    /// <summary>
    /// The path to the fallback image
    /// </summary>
    private static readonly Uri FallbackImagePath = new("avares://WallProjections/Assets/fallback.png");

    /// <inheritdoc />
    public Bitmap? Image
    {
        get => _imageToLoad;
        private set
        {
            this.RaiseAndSetIfChanged(ref _imageToLoad, value);
            this.RaisePropertyChanged(nameof(HasImages));
        }
    }

    /// <inheritdoc />
    public bool HasImages => Image is not null;

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void HideImage()
    {
        Image = null;
    }
}
