using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Platform.Storage;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Test.Mocks.ViewModels.Editor;

public class MockEditorHotspotViewModel : IEditorHotspotViewModel
{
    private Coord _position;
    private string _title;
    private string _description;

    public int Id { get; }

    public Coord Position
    {
        get => _position;
        set
        {
            _position = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Position)));
        }
    }

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
        }
    }

    public ObservableCollection<IThumbnailViewModel> Images { get; }
    public ObservableCollection<IThumbnailViewModel> Videos { get; }

    /// <summary>
    /// Event raised when <see cref="AddMedia" /> is called.
    /// </summary>
    public event EventHandler<MediaAddedEventArgs>? MediaAdded;

    /// <summary>
    /// Event raised when <see cref="RemoveMedia" /> is called.
    /// </summary>
    public event EventHandler<MediaRemovedEventArgs>? MediaRemoved;

    public MockEditorHotspotViewModel(
        int id,
        Coord position,
        string title,
        string description
    ) : this(
        id,
        position,
        title,
        description,
        new ObservableCollection<IThumbnailViewModel>(),
        new ObservableCollection<IThumbnailViewModel>()
    )
    {
    }

    public MockEditorHotspotViewModel(
        int id,
        Coord position,
        string title,
        string description,
        ObservableCollection<IThumbnailViewModel> images,
        ObservableCollection<IThumbnailViewModel> videos
    )
    {
        Id = id;
        _position = position;
        _title = title;
        _description = description;
        Images = images;
        Videos = videos;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Invokes <see cref="PropertyChanged"/> event with the given property name and <i>null</i> sender.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property passed to <see cref="PropertyChangedEventArgs(string)" />.
    /// </param>
    public void UntypedNotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Invokes <see cref="MediaAdded" /> event with the given <paramref name="type" /> and <paramref name="files" />.
    /// </summary>
    /// <remarks><see cref="Images" /> or <see cref="Videos" /> are not actually modified.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="type" /> is not a valid <see cref="MediaEditorType" />.</exception>
    public void AddMedia(MediaEditorType type, IEnumerable<IStorageFile> files)
    {
        if (type > MediaEditorType.Videos)
            throw new ArgumentOutOfRangeException(nameof(type), type, null);

        MediaAdded?.Invoke(this, new MediaAddedEventArgs(type, files));
    }

    /// <summary>
    /// Invokes <see cref="MediaRemoved" /> event with the given <paramref name="type" /> and <paramref name="media" />.
    /// </summary>
    /// <remarks><see cref="Images" /> or <see cref="Videos" /> are not actually modified.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="type" /> is not a valid <see cref="MediaEditorType" />.</exception>
    public void RemoveMedia(MediaEditorType type, IEnumerable<IThumbnailViewModel> media)
    {
        if (type > MediaEditorType.Videos)
            throw new ArgumentOutOfRangeException(nameof(type), type, null);

        MediaRemoved?.Invoke(this, new MediaRemovedEventArgs(type, media));
    }

    public Hotspot ToHotspot()
    {
        const string descPath = "tmp/desc.txt";
        var images = Images.Select(image => image.FilePath).ToImmutableList();
        var videos = Videos.Select(video => video.FilePath).ToImmutableList();
        return new Hotspot(Id, Position, Title, descPath, images, videos);
    }

    /// <summary>
    /// Event arguments for the <see cref="MediaAdded" /> event.
    /// Holds the arguments passed to <see cref="AddMedia" />.
    /// </summary>
    public class MediaAddedEventArgs : EventArgs
    {
        public MediaEditorType Type { get; }
        public ImmutableList<IStorageFile> Files { get; }

        public MediaAddedEventArgs(MediaEditorType type, IEnumerable<IStorageFile> files)
        {
            Type = type;
            Files = files.ToImmutableList();
        }
    }

    /// <summary>
    /// Event arguments for the <see cref="MediaRemoved" /> event.
    /// Holds the arguments passed to <see cref="RemoveMedia" />.
    /// </summary>
    public class MediaRemovedEventArgs : EventArgs
    {
        public MediaEditorType Type { get; }
        public ImmutableList<IThumbnailViewModel> Media { get; }

        public MediaRemovedEventArgs(MediaEditorType type, IEnumerable<IThumbnailViewModel> media)
        {
            Type = type;
            Media = media.ToImmutableList();
        }
    }
}
