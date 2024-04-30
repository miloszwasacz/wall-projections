using System.Collections.Immutable;
using WallProjections.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.ViewModels.Display;
using WallProjections.ViewModels.Display.Layouts;
using static WallProjections.Test.TestExtensions;

namespace WallProjections.Test.ViewModels.Display.Layouts;

[TestFixture]
public class VideoWithDescriptionViewModelTest
{
    [Test]
    public void ConstructorTest()
    {
        const int hotspotId = 0;
        const string title = "Title";
        const string description = "Description";
        IEnumerable<string> videoPaths = new[]
        {
            "video_0.mp4",
            "video_1.mp4",
            "video_2.mp4"
        };
        var vmProvider = new MockViewModelProvider();

        var videoWithDescriptionViewModel = new VideoWithDescriptionViewModel(
            vmProvider,
            hotspotId,
            title,
            description,
            videoPaths
        );
        AssertVMProperties(videoWithDescriptionViewModel, hotspotId, title, description, videoPaths);
    }

    [TestFixture]
    public class FactoryTest : LayoutFactoryTest<VideoWithDescriptionViewModel, VideoWithDescriptionViewModel.Factory>
    {
        private static IEnumerable<TestCaseData<(Hotspot.Media inputMedia, bool expected)>> IsCompatibleDataTestCases =>
            new[]
            {
                ("Compatible", Array.Empty<string>(), new[] { "video_0.mp4" }, true),
                (
                    "CompatibleMultipleVideos",
                    Array.Empty<string>(),
                    new[] { "video_0.mp4", "video_1.mp4", "video_2.mp4" },
                    true
                ),
                ("IncompatibleEmpty", Array.Empty<string>(), Array.Empty<string>(), false),
                ("IncompatibleImages", new[] { "image_0.png" }, Array.Empty<string>(), false),
                ("IncompatibleBoth", new[] { "image_0.png" }, new[] { "video_0.mp4" }, false)
            }.Select((testCase, i) =>
            {
                var (testName, imagePaths, videoPaths, expected) = testCase;
                var media = new Hotspot.Media(
                    i,
                    $"Hotspot {i}",
                    $"Description {i}",
                    imagePaths.ToImmutableList(),
                    videoPaths.ToImmutableList()
                );
                return MakeTestData((media, expected), testName);
            });

        [Test]
        [TestCaseSource(nameof(IsCompatibleDataTestCases))]
        public void IsCompatibleDataTest((Hotspot.Media inputMedia, bool expected) testCase)
        {
            IsCompatibleDataTestBody(testCase);
        }

        [Test]
        [TestCaseSource(nameof(IsCompatibleDataTestCases))]
        public void CreateLayoutTest((Hotspot.Media inputMedia, bool expected) testCase)
        {
            CreateLayoutTestBody(
                testCase,
                (videoWithDescriptionViewModel, media) => AssertVMProperties(
                    videoWithDescriptionViewModel,
                    media.Id,
                    media.Title,
                    media.Description,
                    media.VideoPaths
                )
            );
        }
    }

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Checks if the viewmodel properties are set correctly.
    /// </summary>
    /// <remarks>Disposes of <paramref name="videoWithDescriptionViewModel" /></remarks>
    private static void AssertVMProperties(
        VideoWithDescriptionViewModel videoWithDescriptionViewModel,
        int hotspotId,
        string title,
        string description,
        IEnumerable<string> videoPaths
    )
    {
        Assert.Multiple(() =>
        {
            Assert.Multiple(() =>
            {
                Assert.That(videoWithDescriptionViewModel.HotspotId, Is.EqualTo(hotspotId));
                Assert.That(videoWithDescriptionViewModel.Title, Is.EqualTo(title));
                Assert.That(videoWithDescriptionViewModel.Description, Is.EqualTo(description));
            });

            var videoViewModel = videoWithDescriptionViewModel.VideoViewModel as MockVideoViewModel
                                 ?? throw new InvalidCastException("VideoViewModel is not a mock!");
            Assert.That(videoViewModel.VideoPaths, Is.EquivalentTo(videoPaths));

            videoWithDescriptionViewModel.Dispose();
            Assert.That(videoViewModel.DisposeCount, Is.EqualTo(1));
        });
    }
}
