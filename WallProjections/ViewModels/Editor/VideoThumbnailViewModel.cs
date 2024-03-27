using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using WallProjections.Helper.Interfaces;
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

    /// <inheritdoc />
    public IProcessProxy ProcessProxy { get; }

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
    /// <param name="proxy">A proxy for starting up <see cref="Process" />es.</param>
    public VideoThumbnailViewModel(string path, IProcessProxy proxy)
    {
        ProcessProxy = proxy;
        FilePath = path;
        Image = new Bitmap(AssetLoader.Open(VideoThumbnailPath));
        Name = Path.GetFileName(path);
    }
}
