using Avalonia.ReactiveUI;
using WallProjections.ViewModels;

namespace WallProjections.Views;

public partial class DisplayView : ReactiveUserControl<DisplayViewModel>
{
    public DisplayView()
    {
        InitializeComponent();
    }
}
