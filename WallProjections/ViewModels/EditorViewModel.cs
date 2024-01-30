using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using ReactiveUI;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.ViewModels;

/// <inheritdoc cref="IEditorViewModel" />
public class EditorViewModel : ViewModelBase, IEditorViewModel
{
    /// <summary>
    /// The backing field for <see cref="SelectedHotspot" />.
    /// </summary>
    private IEditorViewModel.IHotspotViewModel? _selectedHotspot;

    /// <inheritdoc />
    public ObservableCollection<IEditorViewModel.IHotspotViewModel> Hotspots { get; }

    /// <inheritdoc />
    public IEditorViewModel.IHotspotViewModel? SelectedHotspot
    {
        get => _selectedHotspot;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedHotspot, value);
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
        }
    }

    /// <inheritdoc />
    public IMediaEditorViewModel ImageEditor { get; } = new MediaEditorViewModel("Images");

    /// <inheritdoc />
    public IMediaEditorViewModel VideoEditor { get; } = new MediaEditorViewModel("Videos");

    /// <inheritdoc />
    public void AddHotspot()
    {
        // Find the smallest available ID
        var newId = 0;
        foreach (var id in Hotspots.Select(h => h.Id).OrderBy(id => id))
        {
            if (newId == id)
                newId++;
        }

        var newHotspot = new HotspotViewModel(newId);
        Hotspots.Add(newHotspot);
        SelectedHotspot = newHotspot;
    }

    /// <summary>
    /// Creates a new empty <see cref="EditorViewModel" />, not linked to any existing <see cref="IConfig" />.
    /// </summary>
    public EditorViewModel()
    {
        Hotspots = new ObservableCollection<IEditorViewModel.IHotspotViewModel>();
    }

    /// <summary>
    /// Creates a new <see cref="EditorViewModel" /> with initial data loaded from the provided <paramref name="config" />.
    /// </summary>
    /// <param name="config">The <see cref="IConfig" /> providing initial state of the editor.</param>
    public EditorViewModel(IConfig config)
    {
        Hotspots = new ObservableCollection<IEditorViewModel.IHotspotViewModel>(
            config.Hotspots.Select(hotspot => new HotspotViewModel(hotspot)));
        SelectedHotspot = Hotspots.FirstOrDefault();
    }

    /// <inheritdoc cref="IEditorViewModel.IHotspotViewModel" />
    private class HotspotViewModel : ViewModelBase, IEditorViewModel.IHotspotViewModel
    {
        /// <inheritdoc />
        public int Id { get; }

        /// <inheritdoc />
        public string Title { get; set; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public ObservableCollection<IThumbnailViewModel> Images { get; }

        /// <inheritdoc />
        public ObservableCollection<IThumbnailViewModel> Videos { get; }

        /// <summary>
        /// Creates a new empty <see cref="HotspotViewModel" /> with the provided <paramref name="id" />.
        /// </summary>
        /// <param name="id">ID of the new hotspot.</param>
        public HotspotViewModel(int id)
        {
            Id = id;
            Title = "";
            Description = "";
            Images = new ObservableCollection<IThumbnailViewModel>();
            Videos = new ObservableCollection<IThumbnailViewModel>();
        }

        /// <summary>
        /// Creates a new <see cref="HotspotViewModel" /> with initial data loaded
        /// from the provided <paramref name="hotspot" />.
        /// </summary>
        /// <param name="hotspot">The base hotspot.</param>
        public HotspotViewModel(Hotspot hotspot)
        {
            Id = hotspot.Id;
            Title = hotspot.Title;
            //TODO Add error handling
            Description = File.ReadAllText(hotspot.DescriptionPath);

            var images = hotspot.ImagePaths
                .Select((path, i) => new ImageThumbnailViewModel(path, GetRow(i), GetColumn(i)));
            var videos = hotspot.VideoPaths
                .Select((path, i) => new VideoThumbnailViewModel(path, GetRow(i), GetColumn(i)));
            Images = new ObservableCollection<IThumbnailViewModel>(images);
            Videos = new ObservableCollection<IThumbnailViewModel>(videos);
            return;

            int GetRow(int index) => index / IMediaEditorViewModel.ColumnCount;
            int GetColumn(int index) => index % IMediaEditorViewModel.ColumnCount;
        }
    }
}
