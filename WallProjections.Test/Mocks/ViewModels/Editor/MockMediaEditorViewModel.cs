using System.Collections.ObjectModel;
using Avalonia.Controls.Selection;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Test.Mocks.ViewModels.Editor;

public class MockMediaEditorViewModel : IMediaEditorViewModel
{
    private ObservableCollection<IThumbnailViewModel> _media;
    public string Title { get; }

    public ObservableCollection<IThumbnailViewModel> Media
    {
        get => _media;
        set
        {
            _media = value;
            SelectedMedia.Source = value;
        }
    }

    public SelectionModel<IThumbnailViewModel> SelectedMedia { get; set; }

    public MockMediaEditorViewModel(MediaEditorType type)
    {
        Title = type.Name();
        _media = new ObservableCollection<IThumbnailViewModel>();
        SelectedMedia = new SelectionModel<IThumbnailViewModel>
        {
            SingleSelect = false,
            Source = _media
        };
    }
}
