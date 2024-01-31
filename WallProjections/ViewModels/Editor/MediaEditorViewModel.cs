using System.Collections.ObjectModel;
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

    /// <inheritdoc />
    public string Title { get; }

    /// <inheritdoc />
    public ObservableCollection<IThumbnailViewModel> Media
    {
        get => _media;
        set => this.RaiseAndSetIfChanged(ref _media, value);
    }

    /// <summary>
    /// Creates a new <see cref="MediaEditorViewModel" /> with the given title
    /// and an empty <see cref="Media" /> collection.
    /// </summary>
    /// <param name="title">The title of the Media Editor.</param>
    public MediaEditorViewModel(string title)
    {
        Title = title;
    }
}
