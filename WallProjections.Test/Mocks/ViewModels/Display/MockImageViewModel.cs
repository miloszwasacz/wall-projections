using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using WallProjections.ViewModels.Display;
using WallProjections.ViewModels.Interfaces.Display;

namespace WallProjections.Test.Mocks.ViewModels.Display;

/// <summary>
/// A mock of <see cref="ImageViewModel" /> for injecting into <see cref="DisplayViewModel" />
/// </summary>
public class MockImageViewModel : IImageViewModel
{
    private readonly List<List<string>> _previousImagePaths = new();

    /// <summary>
    /// The backing field for <see cref="ImagePaths" />
    /// </summary>
    private readonly List<string> _imagePaths = new();

    /// <summary>
    /// A list of paths to the images the viewmodel has shown
    /// </summary>
    public IReadOnlyList<string> ImagePaths => _imagePaths;

    public IReadOnlyList<IReadOnlyList<string>> PreviousImagePaths => _previousImagePaths;

    /// <summary>
    /// The number of times <see cref="AddImages" /> has been called
    /// </summary>
    public int AddCount { get; private set; }

    /// <summary>
    /// The number of times <see cref="ClearImages" /> has been called
    /// </summary>
    public int ClearCount { get; private set; }

    /// <summary>
    /// The number of times <see cref="Dispose"/> has been called
    /// </summary>
    public int DisposeCount { get; private set; }

    public Bitmap? Image => _imagePaths.LastOrDefault() is not null
        ? new Bitmap(PixelFormats.Gray2, AlphaFormat.Opaque, IntPtr.Zero, PixelSize.Empty, Vector.Zero, 0)
        : null;

    /// <inheritdoc />
    public bool HasImages { get; private set; }

    /// <summary>
    /// The number of images currently added.
    /// </summary>
    public int ImageCount => _imagePaths.Count;

    /// <summary>
    /// <i>true</i> once <see cref="StartSlideshow"/> called,
    /// <i>false</i> at initialisation and if <see cref="StopSlideshow"/> called
    /// </summary>
    public bool IsSlideshowRunning { get; private set; }

    /// <summary>
    /// The interval for the currently running slideshow
    /// </summary>
    public TimeSpan? SlideshowInterval { get; private set; }

    /// <summary>
    /// Adds list of new images to current list of images
    /// </summary>
    /// <param name="imagePaths">List of images to add to current list</param>
    public bool AddImages(IEnumerable<string> imagePaths)
    {
        var temp = imagePaths.ToList();
        if (temp.Any())
        {
            AddCount++;
            HasImages = true;
        }

        _imagePaths.AddRange(temp);

        return true;
    }

    /// <summary>
    /// Sets the start slideshow bool to true and store interval.
    /// </summary>
    /// <param name="interval">The current interval for the slideshow stored in <see cref="SlideshowInterval"/></param>
    public void StartSlideshow(TimeSpan? interval)
    {
        IsSlideshowRunning = true;
        SlideshowInterval = interval;
    }

    /// <summary>
    /// Sets the stop slideshow bool to false
    /// </summary>
    public void StopSlideshow()
    {
        IsSlideshowRunning = false;
        SlideshowInterval = null;
    }

    /// <summary>
    /// Increments the <see cref="ClearCount"/> by 1, and moves current images to <see cref="PreviousImagePaths"/>
    /// </summary>
    public void ClearImages()
    {
        _previousImagePaths.Add(_imagePaths);
        _imagePaths.Clear();
        HasImages = false;
        ClearCount++;
    }

    /// <summary>
    /// Increases the number of times
    /// </summary>
    public void Dispose()
    {
        DisposeCount++;
    }
}
