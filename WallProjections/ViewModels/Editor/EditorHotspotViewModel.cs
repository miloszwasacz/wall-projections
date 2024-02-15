using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Platform.Storage;
using DynamicData;
using ReactiveUI;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.ViewModels.Editor;

/// <inheritdoc cref="IEditorHotspotViewModel" />
public class EditorHotspotViewModel : ViewModelBase, IEditorHotspotViewModel
{
    /// <summary>
    /// A <see cref="IViewModelProvider" /> used for creating <see cref="IThumbnailViewModel" />s.
    /// </summary>
    private readonly IViewModelProvider _vmProvider;

    /// <summary>
    /// The backing field for <see cref="Position" />
    /// </summary>
    private Coord _position;

    /// <summary>
    /// The backing field for <see cref="Title" />.
    /// </summary>
    private string _title;

    /// <summary>
    /// The backing field for <see cref="Description" />.
    /// </summary>
    private string _description;

    /// <inheritdoc />
    public int Id { get; }

    /// <inheritdoc />
    public Coord Position
    {
        get => _position;
        set => this.RaiseAndSetIfChanged(ref _position, value);
    }

    /// <inheritdoc />
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    /// <inheritdoc />
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <inheritdoc />
    public ObservableCollection<IThumbnailViewModel> Images { get; }

    /// <inheritdoc />
    public ObservableCollection<IThumbnailViewModel> Videos { get; }

    /// <summary>
    /// Creates a new empty <see cref="EditorHotspotViewModel" /> with the provided <paramref name="id" />.
    /// </summary>
    /// <param name="id">ID of the new hotspot.</param>
    /// <param name="vmProvider">
    /// A <see cref="IViewModelProvider" /> used for creating <see cref="IThumbnailViewModel" />s.
    /// </param>
    public EditorHotspotViewModel(int id, IViewModelProvider vmProvider)
    {
        _vmProvider = vmProvider;
        Id = id;
        _position = new Coord(0, 0, 0);
        _title = "";
        _description = "";
        Images = new ObservableCollection<IThumbnailViewModel>();
        Videos = new ObservableCollection<IThumbnailViewModel>();
    }

    /// <summary>
    /// Creates a new <see cref="EditorHotspotViewModel" /> with initial data loaded
    /// from the provided <paramref name="hotspot" />.
    /// </summary>
    /// <param name="hotspot">The base hotspot.</param>
    /// <param name="vmProvider">
    /// A <see cref="IViewModelProvider" /> used for creating <see cref="IThumbnailViewModel" />s.
    /// </param>
    public EditorHotspotViewModel(Hotspot hotspot, IViewModelProvider vmProvider)
    {
        _vmProvider = vmProvider;
        Id = hotspot.Id;
        _position = hotspot.Position;
        _title = hotspot.Title;
        //TODO Add error handling
        _description = File.ReadAllText(hotspot.FullDescriptionPath);

        var images = hotspot.FullImagePaths.Select(path =>
            _vmProvider.GetThumbnailViewModel(MediaEditorType.Images, path)
        );
        var videos = hotspot.FullVideoPaths.Select(path =>
            _vmProvider.GetThumbnailViewModel(MediaEditorType.Videos, path)
        );
        Images = new ObservableCollection<IThumbnailViewModel>(images);
        Videos = new ObservableCollection<IThumbnailViewModel>(videos);
    }

    /// <inheritdoc />
    public void AddMedia(MediaEditorType type, IEnumerable<IStorageFile> files)
    {
        var collection = type switch
        {
            MediaEditorType.Images => Images,
            MediaEditorType.Videos => Videos,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown media type")
        };

        collection.AddRange(GetIThumbnailViewModels(type, files));
    }

    /// <inheritdoc />
    public void RemoveMedia(MediaEditorType type, IEnumerable<IThumbnailViewModel> media)
    {
        var mediaList = type switch
        {
            MediaEditorType.Images => Images,
            MediaEditorType.Videos => Videos,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown media type")
        };

        mediaList.RemoveMany(media);
    }

    /// <inheritdoc />
    public Hotspot ToHotspot()
    {
        var tempDescPath = Path.GetTempFileName();
        File.WriteAllText(tempDescPath, Description);

        var imagePaths = Images.Select(vm => vm.FilePath).ToImmutableList();
        var videoPaths = Videos.Select(vm => vm.FilePath).ToImmutableList();
        return new Hotspot(Id, Position, Title, tempDescPath, imagePaths, videoPaths);
    }

    /// <summary>
    /// Maps <paramref name="files" /> to <see cref="IThumbnailViewModel" />s
    /// of the appropriate <see cref="MediaEditorType">type</see>.
    /// </summary>
    /// <param name="type">The type of media to map.</param>
    /// <param name="files">The files to map.</param>
    /// <returns>An iterator of <see cref="IThumbnailViewModel" />s.</returns>
    private IEnumerable<IThumbnailViewModel> GetIThumbnailViewModels(
        MediaEditorType type,
        IEnumerable<IStorageFile> files
    ) => files.Select(file => _vmProvider.GetThumbnailViewModel(type, file.Path.AbsolutePath));
}
