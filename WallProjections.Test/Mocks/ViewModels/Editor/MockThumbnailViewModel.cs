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
    public string FilePath { get; }
    public Bitmap Image { get; }
    public string Name { get; }

    public MockThumbnailViewModel(
        string filePath,
        string name,
        Bitmap image,
        IProcessProxy? processProxy = null
    )
    {
        FilePath = filePath;
        Image = image;
        Name = name;
        ProcessProxy = processProxy ?? new MockProcessProxy();
    }

    public MockThumbnailViewModel(string filePath, string name, IProcessProxy? processProxy = null) : this(
        filePath,
        name,
        new Bitmap(PixelFormats.Gray2, AlphaFormat.Opaque, IntPtr.Zero, PixelSize.Empty, Vector.Zero, 0),
        processProxy
    )
    {
    }
}
