using System.Collections;
using Avalonia.Headless.NUnit;
using WallProjections.Helper.Interfaces;
using WallProjections.Test.Mocks.Helper;
using WallProjections.ViewModels.Editor;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Test.ViewModels.Editor;

using ThumbnailVmFactory = Func<string, IProcessProxy, IThumbnailViewModel>;

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
        var path = Path.Combine(TestAssets, file);
        var proxy = new MockProcessProxy();

        var thumbnailViewModel = _constructor(path, proxy);
        Assert.Multiple(() =>
        {
            Assert.That(thumbnailViewModel.FilePath, Is.EqualTo(path));
            Assert.That(thumbnailViewModel.Image, Is.Not.Null);
            Assert.That(thumbnailViewModel.Name, Is.EqualTo(file));
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

        var thumbnailViewModel = _constructor(path, proxy);

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
                new ThumbnailVmFactory((path, proxy) => new ImageThumbnailViewModel(path, proxy)),
                TestImage,
                NonexistentImage
            )
            {
                TestName = "ImageThumbnailViewModel"
            };

            yield return new TestFixtureData(
                new ThumbnailVmFactory((path, proxy) => new VideoThumbnailViewModel(path, proxy)),
                TestVideo,
                NonexistentVideo
            )
            {
                TestName = "VideoThumbnailViewModel"
            };
        }
    }
}
