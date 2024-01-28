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
        /// Description of the hotspot as plain text. The final description
        /// is either this or the contents of <see cref="DescriptionPath"/>,
        /// depending on the value of <see cref="IsDescriptionInFile"/>.
        /// </summary>
        public string Description { get; set; }

        //TODO Maybe load the description from a file instead of linking it to a path?
        /// <summary>
        /// <inheritdoc cref="Hotspot.DescriptionPath"/>
        /// Can be <i>null</i> if the description is not a file <i>(see <see cref="IHotspotViewModel.Description" />)</i>.
        /// </summary>
        public string? DescriptionPath { get; set; }

        /// <inheritdoc cref="Hotspot.ImagePaths"/>
        public ObservableCollection<string> ImagePaths { get; }

        /// <inheritdoc cref="Hotspot.VideoPaths"/>
        public ObservableCollection<string> VideoPaths { get; }

        /// <summary>
        /// Determines whether the final description is plain text or contents of a file.
        /// </summary>
        public bool IsDescriptionInFile { get; set; }

        /// <summary>
        /// A fallback title for the hotspot, used when the title is empty.
        /// </summary>
        public string FallbackTitle => $"Hotspot{Id}";

        /// <summary>
        /// Determines whether the title is empty and should be replaced with a fallback.
        /// </summary>
        public bool IsFallback => Title.Trim() == "";
    }
}
