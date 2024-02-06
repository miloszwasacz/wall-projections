using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Media.Imaging;

namespace WallProjections.ViewModels.Interfaces.Editor;

/// <summary>
/// A viewmodel for an item in <see cref="IMediaEditorViewModel" /> for displaying
/// thumbnails of images and videos associated with a hotspot.
/// </summary>
public interface IThumbnailViewModel
{
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

        //TODO Verify this works on all (necessary) platforms
        // Tested on: Windows 11

        string command;
        if (OperatingSystem.IsWindows())
            command = "explorer.exe";
        else if (OperatingSystem.IsMacOS())
            command = "open";
        else if (OperatingSystem.IsLinux())
            command = "pcmanfm"; // This only handles default Raspbian file manager
        else
            return false;

        Process.Start(command, path);
        return true;
    }
}
