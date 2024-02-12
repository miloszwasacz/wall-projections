using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Platform.Storage;
using WallProjections.Models;

namespace WallProjections.ViewModels.Interfaces.Editor;

/// <summary>
/// A viewmodel for editing data about a <see cref="Hotspot" />.
/// </summary>
public interface IEditorHotspotViewModel
{
    //TODO Add coordinates
    /// <inheritdoc cref="Hotspot.Id" />
    public int Id { get; }

    /// <inheritdoc cref="Hotspot.Position" />
    public Coord Position { get; set; }

    /// <inheritdoc cref="Hotspot.Title" />
    public string Title { get; set; }

    /// <summary>
    /// Description of the hotspot as plain text.
    /// <br /><br />
    /// The contents can be imported from a file.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// A list of paths to images to be displayed in the hotspot wrapped in <see cref="IThumbnailViewModel" />s.
    /// </summary>
    public ObservableCollection<IThumbnailViewModel> Images { get; }

    /// <summary>
    /// A list of paths to videos to be displayed in the hotspot wrapped in <see cref="IThumbnailViewModel" />s.
    /// </summary>
    public ObservableCollection<IThumbnailViewModel> Videos { get; }

    /// <summary>
    /// A fallback title for the hotspot, used when the title is empty.
    /// </summary>
    public string FallbackTitle => $"Hotspot {Id}";

    /// <summary>
    /// Determines whether the title is empty and should be replaced with a fallback.
    /// </summary>
    public bool IsFallback => Title.Trim() == "";

    /// <summary>
    /// Adds media to the appropriate collection (<see cref="Images" /> or <see cref="Videos" />).
    /// </summary>
    /// <param name="type">The <see cref="MediaEditorType">type</see> of media to add.</param>
    /// <param name="files">
    /// An iterator of image files that are converted to <see cref="IThumbnailViewModel" />s and added to the hotspot.
    /// </param>
    public void AddMedia(MediaEditorType type, IEnumerable<IStorageFile> files);

    /// <summary>
    /// Removes media from the appropriate collection (<see cref="Images" /> or <see cref="Videos" />).
    /// </summary>
    /// <param name="type">The <see cref="MediaEditorType">type</see> of media to remove.</param>
    /// <param name="media">The media to remove from the hotspot.</param>
    public void RemoveMedia(MediaEditorType type, IEnumerable<IThumbnailViewModel> media);

    /// <summary>
    /// Creates a <see cref="Hotspot" /> using the currently stored data in this <see cref="IEditorHotspotViewModel" />.
    /// </summary>
    /// <returns>A <see cref="Hotspot" /> representation of this <see cref="IEditorHotspotViewModel" />.</returns>
    public Hotspot ToHotspot();
}
