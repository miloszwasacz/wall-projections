using System;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.ViewModels.Editor;

/// <summary>
/// A viewmodel for an item in <see cref="IMediaEditorViewModel" /> for displaying
/// thumbnails of videos associated with a hotspot.
/// </summary>
public class VideoThumbnailViewModel : ViewModelBase, IThumbnailViewModel
{
    /// <summary>
    /// The <see cref="Uri" /> to the video placeholder image.
    /// </summary>
    private static readonly Uri VideoThumbnailPath = new("avares://WallProjections/Assets/video_placeholder.png");

    /// <summary>
    /// The backing field for <see cref="Row" />.
    /// </summary>
    private int _row;

    /// <summary>
    /// The backing field for <see cref="Column" />.
    /// </summary>
    private int _column;

    /// <inheritdoc />
    public int Row
    {
        get => _row;
        set => this.RaiseAndSetIfChanged(ref _row, value);
    }

    /// <inheritdoc />
    public int Column
    {
        get => _column;
        set => this.RaiseAndSetIfChanged(ref _column, value);
    }

    /// <inheritdoc />
    public string FilePath { get; }

    /// <inheritdoc />
    public Bitmap Image { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>
    /// Creates a new <see cref="VideoThumbnailViewModel" /> with the given path and position in the grid.
    /// </summary>
    /// <param name="path">Path to the associated file.</param>
    /// <param name="row">The row in the grid.</param>
    /// <param name="column">The column in the grid.</param>
    public VideoThumbnailViewModel(string path, int row, int column)
    {
        Row = row;
        Column = column;

        FilePath = path;
        Image = new Bitmap(AssetLoader.Open(VideoThumbnailPath));
        Name = Path.GetFileName(path);
    }
}
