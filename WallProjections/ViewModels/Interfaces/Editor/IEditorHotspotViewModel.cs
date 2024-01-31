using System.Collections.ObjectModel;
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
}
