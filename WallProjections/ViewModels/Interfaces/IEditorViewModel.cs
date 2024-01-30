using System.Collections.ObjectModel;
using WallProjections.Models;
using WallProjections.Models.Interfaces;

namespace WallProjections.ViewModels.Interfaces;

/// <summary>
/// A viewmodel for editing data about a <see cref="IConfig" />.
/// </summary>
public interface IEditorViewModel
{
    /// <summary>
    /// A collection of all hotspots in the editor.
    /// </summary>
    public ObservableCollection<IHotspotViewModel> Hotspots { get; }

    /// <summary>
    /// The currently selected hotspot (or <i>null</i> if <see cref="Hotspots" /> is empty).
    /// </summary>
    public IHotspotViewModel? SelectedHotspot { get; set; }

    /// <summary>
    /// A <see cref="IMediaEditorViewModel" /> for managing images.
    /// </summary>
    public IMediaEditorViewModel ImageEditor { get; }

    /// <summary>
    /// A <see cref="IMediaEditorViewModel" /> for managing videos.
    /// </summary>
    public IMediaEditorViewModel VideoEditor { get; }

    /// <summary>
    /// Adds a new hotspot and selects it.
    /// </summary>
    public void AddHotspot();

    /// <summary>
    /// A viewmodel for editing data about a <see cref="Hotspot"/>.
    /// </summary>
    public interface IHotspotViewModel
    {
        //TODO Add coordinates
        /// <inheritdoc cref="Hotspot.Id"/>
        public int Id { get; }

        /// <inheritdoc cref="Hotspot.Title"/>
        public string Title { get; set; }

        /// <summary>
        /// Description of the hotspot as plain text.
        /// <br /><br />
        /// The contents can be imported from a file.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A list of paths to images to be displayed in the hotspot wrapped in <see cref="IThumbnailViewModel"/>s.
        /// </summary>
        public ObservableCollection<IThumbnailViewModel> Images { get; }

        /// <summary>
        /// A list of paths to videos to be displayed in the hotspot wrapped in <see cref="IThumbnailViewModel"/>s.
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
}
