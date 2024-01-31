using System;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
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
    public int Row { get; }

    /// <inheritdoc />
    public int Column { get; }

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
    /// <param name="row">The row in the grid.</param>
    /// <param name="column">The column in the grid.</param>
    public ImageThumbnailViewModel(string path, int row, int column)
    {
        Row = row;
        Column = column;

        FilePath = path;
        Name = Path.GetFileName(path);

        try
        {
            using var fileStream = File.OpenRead(path);
            Image = new Bitmap(fileStream);
        }
        catch (Exception e)
        {
            //TODO Write to log
            Console.Error.WriteLine(e);
            Image = new Bitmap(AssetLoader.Open(FallbackImagePath));
        }
    }
}
