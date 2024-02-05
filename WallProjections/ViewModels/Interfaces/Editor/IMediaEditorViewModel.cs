using System.Collections.ObjectModel;

namespace WallProjections.ViewModels.Interfaces.Editor;

/// <summary>
/// A viewmodel for the management of media associated with a hotspot.
/// </summary>
public interface IMediaEditorViewModel
{
    /// <summary>
    /// The number of columns in the Media Editor.
    /// </summary>
    public static int ColumnCount => 2;

    /// <summary>
    /// The title of the Media Editor to be displayed as a label.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// A collection of <see cref="IThumbnailViewModel" />s to be displayed in the Media Editor.
    /// </summary>
    public ObservableCollection<IThumbnailViewModel> Media { get; set; }

    /// <summary>
    /// The label of the button to add media.
    /// </summary>
    /// <returns><i>"Add {<see cref="Title" />}"</i></returns>
    public string AddMediaButtonLabel => $"Add {Title}";
}
