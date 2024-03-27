using System.Collections.Immutable;
using WallProjections.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.ViewModels.Display;
using WallProjections.ViewModels.Display.Layouts;
using static WallProjections.Test.TestExtensions;

namespace WallProjections.Test.ViewModels.Display.Layouts;

[TestFixture]
public class ImageWithDescriptionViewModelTest
{
    [AvaloniaTest]
    public void ConstructorTest()
    {
        const string title = "Title";
        const string description = "Description";
        const string imagePath = "image_0.png";
        var vmProvider = new MockViewModelProvider();

        var imageWithDescriptionViewModel = new ImageWithDescriptionViewModel(
            vmProvider,
            title,
            description,
            imagePath
        );
        AssertVMProperties(imageWithDescriptionViewModel, title, description, imagePath);
    }

    [TestFixture]
    public class FactoryTest : LayoutFactoryTest<ImageWithDescriptionViewModel, ImageWithDescriptionViewModel.Factory>
    {
        private static IEnumerable<TestCaseData<(Hotspot.Media inputMedia, bool expected)>> IsCompatibleDataTestCases =>
            new[]
            {
                ("Compatible", new[] { "image_0.png" }, Array.Empty<string>(), true),
                (
                    "CompatibleMultipleImages",
                    new[] { "image_0.png", "image_1.jpg", "image_2.jpeg" },
                    Array.Empty<string>(),
                    true
                ),
                ("IncompatibleEmpty", Array.Empty<string>(), Array.Empty<string>(), false),
                ("IncompatibleVideos", Array.Empty<string>(), new[] { "video_0.mp4" }, false),
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

        [AvaloniaTest]
        [TestCaseSource(nameof(IsCompatibleDataTestCases))]
        public void CreateLayoutTest((Hotspot.Media inputMedia, bool expected) testCase)
        {
            CreateLayoutTestBody(
                testCase,
                (imageWithDescriptionViewModel, media) => AssertVMProperties(
                    imageWithDescriptionViewModel,
                    media.Title,
                    media.Description,
                    media.ImagePaths[0]
                )
            );
        }
    }

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Checks if the viewmodel properties are set correctly.
    /// </summary>
    private static void AssertVMProperties(
        ImageWithDescriptionViewModel imageWithDescriptionViewModel,
        string title,
        string description,
        string imagePath
    )
    {
        Assert.Multiple(() =>
        {
            Assert.That(imageWithDescriptionViewModel.Title, Is.EqualTo(title));
            Assert.That(imageWithDescriptionViewModel.Description, Is.EqualTo(description));
        });

        var imageViewModel = imageWithDescriptionViewModel.ImageViewModel as MockImageViewModel
                             ?? throw new InvalidCastException("ImageViewModel is not a mock!");
        Assert.Multiple(() =>
        {
            Assert.That(imageViewModel.HasImages);
            Assert.That(imageViewModel.ImagePaths, Is.EquivalentTo(new[] { imagePath }));
        });
    }
}
