using System.Diagnostics;
using System.IO;
using Avalonia.Media.Imaging;
using WallProjections.Helper.Interfaces;

namespace WallProjections.ViewModels.Interfaces.Editor;

/// <summary>
/// A viewmodel for an item in <see cref="IMediaEditorViewModel" /> for displaying
/// thumbnails of images and videos associated with a hotspot.
/// </summary>
public interface IThumbnailViewModel
{
    /// <summary>
    /// A proxy for starting up <see cref="Process" />es.
    /// </summary>
    protected IProcessProxy ProcessProxy { get; }

    /// <summary>
    /// The row of the thumbnail in the grid.
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// The column of the thumbnail in the grid.
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// The full path to the associated file.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// The thumbnail image.
    /// </summary>
    public Bitmap Image { get; }

    /// <summary>
    /// The name of the file.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Opens the <see cref="FilePath">associated file</see> in the File Explorer.
    /// </summary>
    public bool OpenInExplorer()
    {
        var path = Directory.GetParent(FilePath)?.FullName ?? FilePath;

        var command = ProcessProxy.GetFileExplorerCommand();
        if (command is null)
            return false;

        ProcessProxy.Start(command, path);
        return true;
    }
}
