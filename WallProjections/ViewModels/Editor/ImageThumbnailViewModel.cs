using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using WallProjections.Helper.Interfaces;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.ViewModels.Editor;

/// <summary>
/// A viewmodel for an item in <see cref="IMediaEditorViewModel" /> for displaying
/// thumbnails of images associated with a hotspot.
/// </summary>
public class ImageThumbnailViewModel : ViewModelBase, IThumbnailViewModel
{
    /// <summary>
    /// The <see cref="Uri" /> to a fallback image.
    /// </summary>
    private static readonly Uri FallbackImagePath = new("avares://WallProjections/Assets/fallback.png");

    /// <inheritdoc />
    public IProcessProxy ProcessProxy { get; }

    /// <inheritdoc />
    public string FilePath { get; }

    /// <inheritdoc />
    public Bitmap Image { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>
    /// Creates a new <see cref="ImageThumbnailViewModel" /> with the given path and position in the grid.
    /// </summary>
    /// <param name="path">Path to the associated file.</param>
    /// <param name="proxy">A proxy for starting up <see cref="Process" />es.</param>
    /// <param name="loggerFactory">A factory for creating loggers.</param>
    public ImageThumbnailViewModel(string path, IProcessProxy proxy, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<ImageThumbnailViewModel>();
        ProcessProxy = proxy;
        FilePath = path;
        Name = Path.GetFileName(path);

        try
        {
            using var fileStream = File.OpenRead(path);
            Image = new Bitmap(fileStream);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to load image from {Path}. Loading fallback image.", path);
            Image = new Bitmap(AssetLoader.Open(FallbackImagePath));
        }
    }
}
