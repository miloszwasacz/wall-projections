using Avalonia.ReactiveUI;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.Views;

public partial class DisplayView : ReactiveUserControl<IDisplayViewModel>
{
    public DisplayView()
    {
        InitializeComponent();
        ImageViewGrid.IsVisible = false;
        VideoViewGrid.IsVisible = false;
    }
}
