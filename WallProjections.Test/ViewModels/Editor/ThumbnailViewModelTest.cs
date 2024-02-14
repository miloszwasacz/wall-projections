using System.Collections;
using System.ComponentModel;
using Avalonia.Headless.NUnit;
using WallProjections.Helper.Interfaces;
using WallProjections.Test.Mocks.Helper;
using WallProjections.ViewModels.Editor;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Test.ViewModels.Editor;

using ThumbnailVmFactory = Func<string, int, int, IProcessProxy, IThumbnailViewModel>;

/// <summary>
/// Tests for <see cref="IThumbnailViewModel" /> based on the <see cref="ThumbnailViewModelFixtureData" />.
/// </summary>
[TestFixtureSource(typeof(ThumbnailViewModelFixtureData), nameof(ThumbnailViewModelFixtureData.FixtureParams))]
public class ThumbnailViewModelTest
{
    private const string TestAssets = "Assets";

    /// <summary>
    /// The constructor to use for creating the appropriate <see cref="IThumbnailViewModel" />.
    /// </summary>
    private readonly ThumbnailVmFactory _constructor;

    /// <summary>
    /// A valid file to use for testing.
    /// </summary>
    private readonly string _testFile;

    /// <summary>
    /// An invalid file to use for testing.
    /// </summary>
    private readonly string _nonexistentFile;

    /// <summary>
    /// Prepares the fixture using the data from <see cref="ThumbnailViewModelFixtureData.FixtureParams" />.
    /// </summary>
    public ThumbnailViewModelTest(ThumbnailVmFactory constructor, string testFile, string nonexistentFile)
    {
        _constructor = constructor;
        _testFile = testFile;
        _nonexistentFile = nonexistentFile;
    }

    [AvaloniaTest]
    [TestCase(true, TestName = "Valid")]
    [TestCase(false, TestName = "Invalid")]
    public void ConstructorTest(bool valid)
    {
        var file = valid ? _testFile : _nonexistentFile;
        const int row = 1;
        const int column = 2;
        var path = Path.Combine(TestAssets, file);
        var proxy = new MockProcessProxy();

        var thumbnailViewModel = _constructor(path, row, column, proxy);
        Assert.Multiple(() =>
        {
            Assert.That(thumbnailViewModel.Row, Is.EqualTo(row));
            Assert.That(thumbnailViewModel.Column, Is.EqualTo(column));
            Assert.That(thumbnailViewModel.FilePath, Is.EqualTo(path));
            Assert.That(thumbnailViewModel.Image, Is.Not.Null);
            Assert.That(thumbnailViewModel.Name, Is.EqualTo(file));
        });
    }


    [AvaloniaTest]
    public void GridPlacementTest()
    {
        const int row = 1;
        const int column = 2;
        var path = Path.Combine(TestAssets, _testFile);
        var proxy = new MockProcessProxy();

        var thumbnailViewModel = _constructor(path, row, column, proxy);

        var rowChanged = false;
        var columnChanged = false;
        if (thumbnailViewModel is INotifyPropertyChanged notifiableVm)
        {
            notifiableVm.PropertyChanged += (_, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(IThumbnailViewModel.Row):
                        rowChanged = true;
                        break;
                    case nameof(IThumbnailViewModel.Column):
                        columnChanged = true;
                        break;
                }
            };
        }
        else Assert.Fail("ViewModel does not implement INotifyPropertyChanged");

        Assert.Multiple(() =>
        {
            Assert.That(thumbnailViewModel.Row, Is.EqualTo(row));
            Assert.That(thumbnailViewModel.Column, Is.EqualTo(column));
        });

        thumbnailViewModel.Row = 3;

        Assert.Multiple(() =>
        {
            Assert.That(rowChanged, Is.True);
            Assert.That(columnChanged, Is.False);
        });

        rowChanged = false;
        thumbnailViewModel.Column = 4;

        Assert.Multiple(() =>
        {
            Assert.That(rowChanged, Is.False);
            Assert.That(columnChanged, Is.True);
        });
    }

    [AvaloniaTest]
    [TestCase(MockProcessProxy.OS.Windows, TestName = "Windows")]
    [TestCase(MockProcessProxy.OS.MacOS, TestName = "MacOS")]
    [TestCase(MockProcessProxy.OS.Linux, TestName = "Linux")]
    [TestCase(null, TestName = "Unknown OS")]
    public void OpenInExplorerTest(MockProcessProxy.OS? os)
    {
        var path = Path.Combine(TestAssets, _testFile);
        var dir = Directory.GetParent(path)!.FullName;
        var proxy = new MockProcessProxy(os);
        var command = proxy.GetFileExplorerCommand();
        var expected = command is not null;

        var thumbnailViewModel = _constructor(path, 1, 2, proxy);

#pragma warning disable NUnit2045
        Assert.That(thumbnailViewModel.OpenInExplorer(), Is.EqualTo(expected));
        Assert.That(
            proxy.LastStart,
            command is not null
                ? Is.EqualTo((command, dir))
                : Is.Null
        );
#pragma warning restore NUnit2045
    }
}

/// <summary>
/// Fixture setup data for <see cref="ThumbnailViewModelTest" />.
/// </summary>
public class ThumbnailViewModelFixtureData
{
    private const string TestImage = "test_image.png";
    private const string NonexistentImage = "nonexistent.png";

    private const string TestVideo = "test_video.mp4";
    private const string NonexistentVideo = "nonexistent.mp4";

    public static IEnumerable FixtureParams
    {
        get
        {
            yield return new TestFixtureData(
                new ThumbnailVmFactory(
                    (path, row, column, proxy) => new ImageThumbnailViewModel(path, row, column, proxy)
                ),
                TestImage,
                NonexistentImage
            )
            {
                TestName = "ImageThumbnailViewModel"
            };

            yield return new TestFixtureData(
                new ThumbnailVmFactory(
                    (path, row, column, proxy) => new VideoThumbnailViewModel(path, row, column, proxy)
                ),
                TestVideo,
                NonexistentVideo
            )
            {
                TestName = "VideoThumbnailViewModel"
            };
        }
    }
}
