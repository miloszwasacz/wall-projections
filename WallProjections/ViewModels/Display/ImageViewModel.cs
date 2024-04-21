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
public class ImageViewModel : ViewModelBase, IImageViewModel, IDisposable
{
    /// <summary>
    /// A logger for this class
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// The path to the fallback image
    /// </summary>
    private static readonly Uri FallbackImagePath = new("avares://WallProjections/Assets/fallback.png");

    private bool _isDisposed;

    private Timer? _slideShowTimer;

    private readonly ObservableCollection<Bitmap> _imageList;

    private int _currentIndex;

    private readonly ObservableAsPropertyHelper<Bitmap?> _image;

    private readonly ObservableAsPropertyHelper<bool> _hasImage;

    private int ImageCount => _imageList.Count;

    private int CurrentIndex
    {
        get => _currentIndex;
        set => this.RaiseAndSetIfChanged(ref _currentIndex, value);
    }

    /// <summary>
    /// Creates a new <see cref="ImageViewModel" />
    /// </summary>
    /// <param name="loggerFactory">A factory for creating loggers</param>
    public ImageViewModel(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ImageViewModel>();
        _currentIndex = 0;

        _imageList = new ObservableCollection<Bitmap>();

        _image = this.WhenAnyValue(x => x.CurrentIndex, x => x._imageList.Count, (index, imageList) =>
        {
            if (ImageCount == 0)
                return null;
            
            if (index >= ImageCount || index < 0)
                return new Bitmap(AssetLoader.Open(FallbackImagePath));
            
            return _imageList[index];
        }).ToProperty(this, x => x.Image);
        
        _hasImage = this.WhenAnyValue(x => x._imageList.Count, imageList => imageList > 0)
            .ToProperty(this, x => x.HasImages);
    }

    /// <inheritdoc />
    public Bitmap? Image => _image.Value;

    /// <inheritdoc />
    public bool HasImages => _hasImage.Value;

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
        
        // 20 Seconds by default for slideshow.
        var i = interval ?? TimeSpan.FromSeconds(20);
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
        var copy = new List<Bitmap>(_imageList);
        _imageList.Clear();
        this.RaisePropertyChanged(nameof(_imageList));
        
        foreach (var image in copy)
        {
            image.Dispose();
        }
    }


    public void Dispose()
    {
        if (_isDisposed)
            return;
        
        _isDisposed = true;
        StopSlideshow();
        ClearImages();
    }
}
