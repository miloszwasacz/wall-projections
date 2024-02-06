﻿using System.Collections.ObjectModel;
using Avalonia.Controls.Selection;

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
    /// A collection of selected <see cref="IThumbnailViewModel" />s.
    /// </summary>
    public SelectionModel<IThumbnailViewModel> SelectedMedia { get; set; }

    /// <summary>
    /// Whether media can be removed from the Media Editor.
    /// </summary>
    public bool CanRemoveMedia => Media.Count > 0 && SelectedMedia.Count > 0;

    /// <summary>
    /// The label of the button to add media.
    /// </summary>
    /// <returns><i>"Add {<see cref="Title" />}"</i></returns>
    public string AddMediaButtonLabel => $"Add {Title}";

    /// <summary>
    /// The label of the button to remove media.
    /// </summary>
    /// <returns><i>"Remove {<see cref="Title" />}"</i></returns>
    public string RemoveMediaButtonLabel => $"Remove {Title}";
}

/// <summary>
/// The type of media that a hotspot can have.
/// </summary>
public enum MediaEditorType
{
    Images,
    Videos
}
