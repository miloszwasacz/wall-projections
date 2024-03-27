using System;

namespace WallProjections.ViewModels.Interfaces.Editor;

/// <summary>
/// A viewmodel for editing the title and description of a hotspot.
/// </summary>
public interface IDescriptionEditorViewModel
{
    /// <summary>
    /// An event that is raised when the content of the <see cref="Hotspot" />
    /// (i.e. <see cref="IEditorHotspotViewModel.Title" /> or <see cref="IEditorHotspotViewModel.Description"/>)
    /// has changed.
    /// </summary>
    /// <remarks>Note that this event is not raised when the <see cref="Hotspot" /> that is edited changes.</remarks>
    public event EventHandler<EventArgs>? ContentChanged;

    /// <summary>
    /// The source <see cref="IEditorHotspotViewModel">hotspot</see> that is being edited.
    /// </summary>
    public IEditorHotspotViewModel? Hotspot { set; }

    /// <summary>
    /// The title of the hotspot that is being edited.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The description of the hotspot that is being edited.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Whether hotspot's details can be edited.
    /// </summary>
    public bool IsEnabled { get; }

    /// <summary>
    /// A viewmodel for importing hotspot's title and description from a file.
    /// </summary>
    public IImportViewModel Importer { get; }
}
