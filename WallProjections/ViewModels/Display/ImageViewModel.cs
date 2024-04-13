using System;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using WallProjections.ViewModels.Interfaces.Display;

namespace WallProjections.ViewModels.Display;

/// <inheritdoc cref="IImageViewModel" />
public class ImageViewModel : ViewModelBase, IImageViewModel
{
    /// <summary>
    /// A logger for this class
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// The backing field for <see cref="Image" />
    /// </summary>
    private Bitmap? _imageToLoad;

    /// <summary>
    /// The path to the fallback image
    /// </summary>
    private static readonly Uri FallbackImagePath = new("avares://WallProjections/Assets/fallback.png");

    /// <summary>
    /// Creates a new <see cref="ImageViewModel" />
    /// </summary>
    /// <param name="loggerFactory">A factory for creating loggers</param>
    public ImageViewModel(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ImageViewModel>();
    }

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
            _logger.LogWarning("Image file not found: {FilePath}", filePath);
            Image = new Bitmap(AssetLoader.Open(FallbackImagePath));
            return false;
        }

        try
        {
            using var fileStream = File.OpenRead(filePath);
            Image = new Bitmap(fileStream);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load image: {FilePath}", filePath);
            Image = new Bitmap(AssetLoader.Open(FallbackImagePath));
            return false;
        }
    }

    /// <inheritdoc />
    public void HideImage()
    {
        Image = null;
    }
}
