using Avalonia.Controls;

namespace WallProjections.Views;

public partial class VideoView : UserControl
{
    public VideoView()
    {
        InitializeComponent();
    }

    internal void OnResize(object? sender, SizeChangedEventArgs e)
    {
        if (e.WidthChanged)
            Height = e.NewSize.Width * 9 / 16;
    }
}
