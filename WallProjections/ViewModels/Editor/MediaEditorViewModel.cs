using System;
using System.Collections.ObjectModel;
using Avalonia.Controls.Selection;
using ReactiveUI;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.ViewModels.Editor;

/// <inheritdoc cref="IMediaEditorViewModel" />
public class MediaEditorViewModel : ViewModelBase, IMediaEditorViewModel
{
    /// <summary>
    /// The backing field for <see cref="Media" />.
    /// </summary>
    private ObservableCollection<IThumbnailViewModel> _media = new();

    /// <summary>
    /// The backing field for <see cref="SelectedMedia" />.
    /// </summary>
    private SelectionModel<IThumbnailViewModel> _selectedMedia;

    /// <inheritdoc />
    public string Title { get; }

    /// <inheritdoc />
    public ObservableCollection<IThumbnailViewModel> Media
    {
        get => _media;
        set
        {
            Media.CollectionChanged -= UpdateCanRemoveMedia;
            Media.CollectionChanged -= ClearSelectedMedia;
            this.RaiseAndSetIfChanged(ref _media, value);
            SelectedMedia.Source = Media;
            this.RaisePropertyChanged(nameof(SelectedMedia));
            Media.CollectionChanged += UpdateCanRemoveMedia;
            Media.CollectionChanged += ClearSelectedMedia;
        }
    }

    /// <inheritdoc />
    public SelectionModel<IThumbnailViewModel> SelectedMedia
    {
        get => _selectedMedia;
        set
        {
            value.Source = Media;
            this.RaiseAndSetIfChanged(ref _selectedMedia, value);
        }
    }

    /// <summary>
    /// Creates a new <see cref="MediaEditorViewModel" /> with the given title
    /// and an empty <see cref="Media" /> collection.
    /// </summary>
    /// <param name="title">The title of the Media Editor.</param>
    public MediaEditorViewModel(string title)
    {
        Title = title;
        _selectedMedia = new SelectionModel<IThumbnailViewModel>
        {
            SingleSelect = false,
            Source = Media
        };

        Media.CollectionChanged += UpdateCanRemoveMedia;
        Media.CollectionChanged += ClearSelectedMedia;
        SelectedMedia.SelectionChanged += UpdateCanRemoveMedia;
    }

    // ReSharper disable UnusedParameter.Local

    /// <summary>
    /// Handler for the <see cref="ObservableCollection{T}.CollectionChanged" /> event of <see cref="Media" />
    /// and the <see cref="SelectionModel{T}.SelectionChanged" /> event of <see cref="SelectedMedia" />.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private void UpdateCanRemoveMedia(object? sender, EventArgs e)
    {
        this.RaisePropertyChanged(nameof(IMediaEditorViewModel.CanRemoveMedia));
    }

    /// <summary>
    /// Handler for the <see cref="ObservableCollection{T}.CollectionChanged" /> event of <see cref="Media" />
    /// to clear the <see cref="SelectedMedia" />.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private void ClearSelectedMedia(object? sender, EventArgs e)
    {
        SelectedMedia.Clear();
    }

    // ReSharper restore UnusedParameter.Local
}
