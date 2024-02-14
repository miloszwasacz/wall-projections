using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using WallProjections.Helper.Interfaces;
using WallProjections.Test.Mocks.Helper;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Test.Mocks.ViewModels.Editor;

public class MockThumbnailViewModel : IThumbnailViewModel
{
    public IProcessProxy ProcessProxy { get; }
    public int Row { get; set; }
    public int Column { get; set; }
    public string FilePath { get; }
    public Bitmap Image { get; }
    public string Name { get; }

    public MockThumbnailViewModel(
        int row,
        int column,
        string filePath,
        string name,
        Bitmap image,
        IProcessProxy? processProxy = null
    )
    {
        Row = row;
        Column = column;
        FilePath = filePath;
        Image = image;
        Name = name;
        ProcessProxy = processProxy ?? new MockProcessProxy();
    }

    public MockThumbnailViewModel(int row, int column, string filePath, string name,
        IProcessProxy? processProxy = null) : this(
        row,
        column,
        filePath,
        name,
        new Bitmap(PixelFormats.Gray2, AlphaFormat.Opaque, IntPtr.Zero, PixelSize.Empty, Vector.Zero, 0),
        processProxy
    )
    {
    }
}
