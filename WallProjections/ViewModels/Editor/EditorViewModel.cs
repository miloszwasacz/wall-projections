using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Platform.Storage;
using DynamicData;
using ReactiveUI;
using WallProjections.Helper;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.ViewModels.Editor;

/// <inheritdoc cref="IEditorViewModel{T}" />
public class EditorViewModel : ViewModelBase, IEditorViewModel
{
    /// <summary>
    /// A <see cref="IFileHandler" /> used for saving, importing, and exporting configs.
    /// </summary>
    private readonly IFileHandler _fileHandler;

    /// <summary>
    /// The backing field for <see cref="Hotspots" />.
    /// </summary>
    private ObservableHotspotCollection<EditorHotspotViewModel> _hotspots;

    /// <summary>
    /// The backing field for <see cref="SelectedHotspot" />.
    /// </summary>
    private EditorHotspotViewModel? _selectedHotspot;

    /// <summary>
    /// The backing field for <see cref="DescriptionEditor" />.
    /// </summary>
    private readonly DescriptionEditorViewModel _descriptionEditor = new();

    /// <summary>
    /// The backing field for <see cref="IsSaved" />.
    /// </summary>
    private bool _isSaved;

    /// <inheritdoc />
    public ObservableHotspotCollection<EditorHotspotViewModel> Hotspots
    {
        get => _hotspots;
        set
        {
            _hotspots.CollectionChanged -= SetUnsaved;

            this.RaiseAndSetIfChanged(ref _hotspots, value);
            SelectedHotspot = null;

            _hotspots.CollectionChanged += SetUnsaved;
            IsSaved = false;
        }
    }

    /// <inheritdoc />
    public EditorHotspotViewModel? SelectedHotspot
    {
        get => _selectedHotspot;
        set
        {
            // Skip changes to SelectedItem if the collection is only updating because of PropertyChanged on an item
            if (Hotspots.IsItemUpdating) return;

            this.RaiseAndSetIfChanged(ref _selectedHotspot, value);
            _descriptionEditor.Hotspot = value;

            ImageEditor.Media.CollectionChanged -= SetUnsaved;
            VideoEditor.Media.CollectionChanged -= SetUnsaved;

            if (value is not null)
            {
                ImageEditor.Media = value.Images;
                VideoEditor.Media = value.Videos;
            }
            else
            {
                ImageEditor.Media = new ObservableCollection<IThumbnailViewModel>();
                VideoEditor.Media = new ObservableCollection<IThumbnailViewModel>();
            }

            ImageEditor.Media.CollectionChanged += SetUnsaved;
            VideoEditor.Media.CollectionChanged += SetUnsaved;
        }
    }

    /// <inheritdoc />
    public IDescriptionEditorViewModel DescriptionEditor => _descriptionEditor;

    /// <inheritdoc />
    public IMediaEditorViewModel ImageEditor { get; } = new MediaEditorViewModel("Images");

    /// <inheritdoc />
    public IMediaEditorViewModel VideoEditor { get; } = new MediaEditorViewModel("Videos");

    /// <inheritdoc />
    public bool IsSaved
    {
        get => _isSaved;
        set
        {
            this.RaiseAndSetIfChanged(ref _isSaved, value);
            this.RaisePropertyChanged(nameof(IEditorViewModel.CloseButtonText));
        }
    }

    /// <inheritdoc />
    public void AddHotspot()
    {
        IsSaved = false;

        // Find the smallest available ID
        var newId = 0;
        foreach (var id in Hotspots.Select(h => h.Id).OrderBy(id => id))
        {
            if (newId == id)
                newId++;
        }

        var newHotspot = new EditorHotspotViewModel(newId);
        Hotspots.Add(newHotspot);
        SelectedHotspot = newHotspot;
    }

    /// <inheritdoc />
    public void DeleteHotspot(EditorHotspotViewModel hotspot)
    {
        IsSaved = false;

        Hotspots.Remove(hotspot);
        SelectedHotspot = Hotspots.FirstOrDefault();
    }

    /// <inheritdoc />
    public void AddMedia(MediaEditorType type, IEnumerable<IStorageFile> files)
    {
        if (SelectedHotspot is null) return;
        IsSaved = false;

        SelectedHotspot?.AddMedia(type, files);
    }

    /// <inheritdoc />
    public void RemoveMedia(MediaEditorType type, IEnumerable<IThumbnailViewModel> media)
    {
        if (SelectedHotspot is null) return;
        IsSaved = false;

        var selectedMedia = type switch
        {
            MediaEditorType.Images => ImageEditor.SelectedMedia,
            MediaEditorType.Videos => VideoEditor.SelectedMedia,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        SelectedHotspot?.RemoveMedia(type, media);
        selectedMedia.Clear();
    }

    /// <inheritdoc />
    public bool SaveConfig()
    {
        try
        {
            var hotspots = Hotspots.Select(hotspot => hotspot.ToHotspot());
            var config = new Config(hotspots);

            // Set IsSaved to the result of SaveConfig and return the same value
            return IsSaved = _fileHandler.SaveConfig(config);
        }
        catch (Exception e)
        {
            //TODO Log to file
            Console.Error.WriteLine(e);
            return false;
        }
    }

    /// <inheritdoc />
    public bool ImportConfig(string filePath)
    {
        try
        {
            var config = _fileHandler.ImportConfig(filePath);
            if (config is null) return false;

            Hotspots = new ObservableHotspotCollection<EditorHotspotViewModel>(
                config.Hotspots.Select(hotspot => new EditorHotspotViewModel(hotspot)));
            SelectedHotspot = Hotspots.FirstOrDefault();

            IsSaved = true;
            return true;
        }
        catch (Exception e)
        {
            //TODO Log to file
            Console.Error.WriteLine(e);
            return false;
        }
    }

    /// <inheritdoc />
    public bool ExportConfig(string exportPath)
    {
        //TODO Implement export functionality (FileHandler does not have export functionality yet)
        return false;
    }

    /// <summary>
    /// Creates a new empty <see cref="EditorViewModel" />, not linked to any existing <see cref="IConfig" />.
    /// </summary>
    /// <param name="fileHandler">A <see cref="IFileHandler" /> used for saving, importing, and exporting configs.</param>
    public EditorViewModel(IFileHandler fileHandler)
    {
        _fileHandler = fileHandler;
        _hotspots = new ObservableHotspotCollection<EditorHotspotViewModel>();

        InitEventHandlers();
    }

    /// <summary>
    /// Creates a new <see cref="EditorViewModel" /> with initial data loaded from the provided <paramref name="config" />.
    /// </summary>
    /// <param name="config">The <see cref="IConfig" /> providing initial state of the editor.</param>
    /// <param name="fileHandler">A <see cref="IFileHandler" /> used for saving, importing, and exporting configs.</param>
    public EditorViewModel(IConfig config, IFileHandler fileHandler)
    {
        _fileHandler = fileHandler;
        _hotspots = new ObservableHotspotCollection<EditorHotspotViewModel>(
            config.Hotspots.Select(hotspot => new EditorHotspotViewModel(hotspot)));
        SelectedHotspot = Hotspots.FirstOrDefault();
        IsSaved = true;

        InitEventHandlers();
    }

    /// <summary>
    /// Initializes event handlers for the editor (to keep track of unsaved changes).
    /// </summary>
    private void InitEventHandlers()
    {
        _hotspots.CollectionChanged += SetUnsaved;
        _descriptionEditor.ContentChanged += SetUnsaved;
        ImageEditor.Media.CollectionChanged += SetUnsaved;
        VideoEditor.Media.CollectionChanged += SetUnsaved;
    }

    /// <summary>
    /// A callback that sets <see cref="IsSaved" /> to <i>false</i>.
    /// </summary>
    private void SetUnsaved(object? sender, EventArgs e)
    {
        IsSaved = false;
    }

    /// <inheritdoc cref="IEditorHotspotViewModel" />
    public class EditorHotspotViewModel : ViewModelBase, IEditorHotspotViewModel
    {
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
        public EditorHotspotViewModel(int id)
        {
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
        public EditorHotspotViewModel(Hotspot hotspot)
        {
            Id = hotspot.Id;
            _position = hotspot.Position;
            _title = hotspot.Title;
            //TODO Add error handling
            _description = File.ReadAllText(hotspot.FullDescriptionPath);

            var images = hotspot.FullImagePaths
                .Select((path, i) => new ImageThumbnailViewModel(path, GetMediaRow(i), GetMediaColumn(i)));
            var videos = hotspot.FullVideoPaths
                .Select((path, i) => new VideoThumbnailViewModel(path, GetMediaRow(i), GetMediaColumn(i)));
            Images = new ObservableCollection<IThumbnailViewModel>(images);
            Videos = new ObservableCollection<IThumbnailViewModel>(videos);
        }

        /// <inheritdoc />
        public void AddMedia(MediaEditorType type, IEnumerable<IStorageFile> files)
        {
            switch (type)
            {
                case MediaEditorType.Images:
                    Images.AddRange(
                        GetIThumbnailViewModels(
                            files,
                            Images.Count,
                            (path, row, column) => new ImageThumbnailViewModel(path, row, column)
                        )
                    );
                    break;
                case MediaEditorType.Videos:
                    Videos.AddRange(
                        GetIThumbnailViewModels(
                            files,
                            Videos.Count,
                            (path, row, column) => new VideoThumbnailViewModel(path, row, column)
                        )
                    );
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown media type");
            }
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
            ReindexMedia(mediaList);
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
        /// using the provided <paramref name="factory" />.
        /// </summary>
        /// <param name="files">The files to map.</param>
        /// <param name="indexOffset">
        /// An offset added to the index of each file (used for correct placement in the grid).
        /// </param>
        /// <param name="factory">The factory used to create <see cref="IThumbnailViewModel" />s.</param>
        /// <returns>An iterator of <see cref="IThumbnailViewModel" />s.</returns>
        private static IEnumerable<IThumbnailViewModel> GetIThumbnailViewModels(
            IEnumerable<IStorageFile> files,
            int indexOffset,
            Func<string, int, int, IThumbnailViewModel> factory
        ) => files.Select((file, i) =>
        {
            var path = file.Path.AbsolutePath;
            var index = i + indexOffset;
            var row = GetMediaRow(index);
            var column = GetMediaColumn(index);
            return factory(path, row, column);
        });

        /// <summary>
        /// Updates the <see cref="IThumbnailViewModel.Row" /> and <see cref="IThumbnailViewModel.Column" />
        /// of the media items in the given <paramref name="media" /> collection.
        /// </summary>
        /// <param name="media">The collection of media items to reindex.</param>
        private static void ReindexMedia(IReadOnlyList<IThumbnailViewModel> media)
        {
            for (var i = 0; i < media.Count; i++)
            {
                var thumbnail = media[i];
                thumbnail.Row = GetMediaRow(i);
                thumbnail.Column = GetMediaColumn(i);
            }
        }

        /// <summary>
        /// Returns the row of the grid where the media item at the given <paramref name="index" /> is.
        /// </summary>
        /// <returns><paramref name="index" /> divided by <see cref="IMediaEditorViewModel.ColumnCount" /></returns>
        private static int GetMediaRow(int index) => index / IMediaEditorViewModel.ColumnCount;

        /// <summary>
        /// Returns the column of the grid where the media item at the given <paramref name="index" /> is.
        /// </summary>
        /// <returns><paramref name="index" /> modulo <see cref="IMediaEditorViewModel.ColumnCount" /></returns>
        private static int GetMediaColumn(int index) => index % IMediaEditorViewModel.ColumnCount;
    }
}
