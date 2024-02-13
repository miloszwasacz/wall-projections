using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Test.Mocks.ViewModels.Editor;

public class MockThumbnailViewModel : IThumbnailViewModel
{
    public int Row { get; set; } = 0;
    public int Column { get; set; } = 0;
    public string FilePath { get; }
    public Bitmap Image { get; }
    public string Name { get; }

    public MockThumbnailViewModel(int row, int column, string filePath, string name, Bitmap image)
    {
        Row = row;
        Column = column;
        FilePath = filePath;
        Image = image;
        Name = name;
    }

    public MockThumbnailViewModel(int row, int column, string filePath, string name)
    {
        Row = row;
        Column = column;
        FilePath = filePath;
        Image = new Bitmap(PixelFormats.Gray2, AlphaFormat.Opaque, IntPtr.Zero, PixelSize.Empty, Vector.Zero, 0);
        Name = name;
    }
}
