using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DynamicData;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using WallProjections.ViewModels.Interfaces.Display;

namespace WallProjections.ViewModels.Display;

/// <inheritdoc cref="IImageViewModel" />
public class ImageViewModel : ViewModelBase, IImageViewModel
{
    /// <summary>
    /// The path to the fallback image
    /// </summary>
    private static readonly Uri FallbackImagePath = new("avares://WallProjections/Assets/fallback.png");

    /// <summary>
    /// A logger for this class
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// If <see cref="ImageViewModel"/> is disposed.
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// Timer for the slideshow while slideshow is running using <see cref="StartSlideshow"/>.
    /// </summary>
    private Timer? _slideShowTimer;

    /// <summary>
    /// Stores all current images.
    /// </summary>
    private readonly ObservableCollection<Bitmap> _imageList;

    /// <summary>
    /// Backing field for <see cref="CurrentIndex"/>
    /// </summary>
    private int _currentIndex;

    /// <summary>
    /// Backing field for <see cref="Image"/>
    /// </summary>
    private readonly ObservableAsPropertyHelper<Bitmap?> _image;

    /// <summary>
    /// Backing field for <see cref="HasImages"/>
    /// </summary>
    private readonly ObservableAsPropertyHelper<bool> _hasImages;

    /// <summary>
    /// The number of images currently added.
    /// </summary>
    public int ImageCount => _imageList.Count;

    /// <summary>
    /// The current index of the image to be displayed on the screen.
    /// </summary>
    private int CurrentIndex
    {
        get => _currentIndex;
        set => this.RaiseAndSetIfChanged(ref _currentIndex, value);
    }
    
    /// <inheritdoc />
    public Bitmap? Image => _image.Value;

    /// <inheritdoc />
    public bool HasImages => _hasImages.Value;

    /// <summary>
    /// Creates a new <see cref="ImageViewModel" />
    /// </summary>
    /// <param name="loggerFactory">A factory for creating loggers</param>
    public ImageViewModel(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ImageViewModel>();
        _currentIndex = 0;

        _imageList = new ObservableCollection<Bitmap>();

        _image = GetImageProperty();
        _hasImages = GetHasImagesProperty();
    }

    /// <inheritdoc />
    public bool AddImages(IEnumerable<string> imagePaths)
    {
        var temp = new List<Bitmap>();
        var fullSuccess = true;
        foreach (var imagePath in imagePaths)
        {
            try
            {
                using var fileStream = File.OpenRead(imagePath);
                temp.Add(new Bitmap(fileStream));
            }
            catch (FileNotFoundException)
            {
                _logger.LogWarning("Image file not found: {FilePath}", imagePath);
                fullSuccess = false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to load image: {FilePath}", imagePath);
                fullSuccess = false;
            }
        }

        _imageList.AddRange(temp);
        return fullSuccess;
    }

    /// <inheritdoc />
    public void StartSlideshow(TimeSpan? interval = null)
    {
        if (_slideShowTimer is not null || ImageCount <= 1)
            return;
        
        if (ImageCount == 0)
            throw new InvalidOperationException("Cannot slideshow with no images");

        var i = interval ?? IImageViewModel.DefaultImageInterval;
        
        _slideShowTimer = new Timer(_ =>
        {
            CurrentIndex = (CurrentIndex + 1) % ImageCount;
        }, null, i, i);
    }

    /// <inheritdoc />
    public void StopSlideshow()
    {
        if (_slideShowTimer is null) return;
        
        _slideShowTimer.Dispose();
        _slideShowTimer = null;
    }

    /// <inheritdoc />
    public void ClearImages()
    {
        StopSlideshow();
        
        var copy = new List<Bitmap>(_imageList);
        _imageList.Clear();
        this.RaisePropertyChanged(nameof(_imageList));
        
        foreach (var image in copy)
        {
            image.Dispose();
        }
    }
    
    /// <summary>
    /// Creates a new observable property for <see cref="_image"/>
    /// </summary>
    private ObservableAsPropertyHelper<Bitmap?> GetImageProperty() => this
        .WhenAnyValue(x => x.CurrentIndex, x => x._imageList.Count, (index, imageCount) =>
        {
            if (imageCount == 0)
                return null;
                
            if (index >= imageCount || index < 0)
                return new Bitmap(AssetLoader.Open(FallbackImagePath));
                
            return _imageList[index];
        })
        .ToProperty(this, x => x.Image);
    
    /// <summary>
    /// Creates a new observable property for <see cref="_hasImages"/>
    /// </summary>
    private ObservableAsPropertyHelper<bool> GetHasImagesProperty() => this
        .WhenAnyValue(x => x._imageList.Count, imageList => imageList > 0)
        .ToProperty(this, x => x.HasImages);
 

    public void Dispose()
    {
        if (_isDisposed)
            return;
        
        _isDisposed = true;
        StopSlideshow();
        ClearImages();
        GC.SuppressFinalize(this);
    }
}
