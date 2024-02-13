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

    public MockEditorHotspotViewModel(
        int id,
        Coord position,
        string title,
        string description
    )
    {
        Id = id;
        _position = position;
        _title = title;
        _description = description;
        Images = new ObservableCollection<IThumbnailViewModel>();
        Videos = new ObservableCollection<IThumbnailViewModel>();
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

    public void AddMedia(MediaEditorType type, IEnumerable<IStorageFile> files)
    {
        throw new NotImplementedException();
    }

    public void RemoveMedia(MediaEditorType type, IEnumerable<IThumbnailViewModel> media)
    {
        throw new NotImplementedException();
    }

    public Hotspot ToHotspot()
    {
        throw new NotImplementedException();
    }
}
