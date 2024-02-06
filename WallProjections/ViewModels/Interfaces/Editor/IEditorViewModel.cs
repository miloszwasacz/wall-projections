using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Platform.Storage;
using WallProjections.Helper;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Editor;

namespace WallProjections.ViewModels.Interfaces.Editor;

/// <summary>
/// A viewmodel for editing data about a <see cref="IConfig" />.
/// </summary>
/// <typeparam name="T">The type of <see cref="Hotspots" />' items.</typeparam>
public interface IEditorViewModel<T>
    where T : IEditorHotspotViewModel, INotifyPropertyChanged
{
    /// <summary>
    /// A collection of all hotspots in the editor.
    /// </summary>
    public ObservableHotspotCollection<T> Hotspots { get; }

    /// <summary>
    /// The currently selected hotspot (or <i>null</i> if <see cref="Hotspots" /> is empty).
    /// </summary>
    public T? SelectedHotspot { get; set; }

    /// <summary>
    /// A <see cref="IDescriptionEditorViewModel" /> for editing the title and description of the currently selected hotspot.
    /// </summary>
    public IDescriptionEditorViewModel DescriptionEditor { get; }

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
    /// Deletes the given hotspot.
    /// </summary>
    /// <param name="hotspot">The hotspot to delete.</param>
    public void DeleteHotspot(T hotspot);

    /// <summary>
    /// Adds media of the given <paramref name="type" /> to <see cref="SelectedHotspot" />.
    /// </summary>
    /// <param name="type">The <see cref="MediaEditorType">type</see> of media to add.</param>
    /// <param name="files">
    /// An iterator of image files that are converted to <see cref="IThumbnailViewModel" />s and added to the hotspot.
    /// </param>
    public void AddMedia(MediaEditorType type, IEnumerable<IStorageFile> files);

    /// <summary>
    /// Removes appropriate media from <see cref="SelectedHotspot" />.
    /// </summary>
    /// <param name="type">The <see cref="MediaEditorType">type</see> of media to remove.</param>
    /// <param name="media">The media to remove from the hotspot.</param>
    public void RemoveMedia(MediaEditorType type, IEnumerable<IThumbnailViewModel> media);
}

/// <summary>
/// An extension of <see cref="IEditorViewModel{T}" /> with <see cref="EditorViewModel.EditorHotspotViewModel" />
/// as the generic type (required for XAML compatibility).
/// </summary>
public interface IEditorViewModel : IEditorViewModel<EditorViewModel.EditorHotspotViewModel>
{
}
