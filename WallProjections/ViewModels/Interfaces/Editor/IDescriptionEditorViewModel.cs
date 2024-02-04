namespace WallProjections.ViewModels.Interfaces.Editor;

/// <summary>
/// A viewmodel for editing the title and description of a hotspot.
/// </summary>
public interface IDescriptionEditorViewModel
{
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
