using Avalonia.Controls;

namespace WallProjections.Views;

public partial class VideoView : UserControl
{
    public VideoView()
    {
        InitializeComponent();
    }

    // ReSharper disable UnusedParameter.Local
    private void OnResize(object? sender, SizeChangedEventArgs e)
    {
        if (e.WidthChanged)
            Height = e.NewSize.Width * 9 / 16;
    }
}
