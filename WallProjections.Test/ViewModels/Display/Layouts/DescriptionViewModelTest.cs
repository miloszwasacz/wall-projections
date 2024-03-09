using System.Collections.Immutable;
using WallProjections.Models;
using WallProjections.ViewModels.Display.Layouts;
using static WallProjections.Test.TestExtensions;

namespace WallProjections.Test.ViewModels.Display.Layouts;

[TestFixture]
public class DescriptionViewModelTest
{
    [Test]
    public void ConstructorTest()
    {
        const string title = "Title";
        const string description = "Description";

        var descriptionViewModel = new DescriptionViewModel(title, description);
        Assert.Multiple(() =>
        {
            Assert.That(descriptionViewModel.Title, Is.EqualTo(title));
            Assert.That(descriptionViewModel.Description, Is.EqualTo(description));
        });
    }

    [TestFixture]
    public class FactoryTest : LayoutFactoryTest<DescriptionViewModel, DescriptionViewModel.Factory>
    {
        private static IEnumerable<TestCaseData<(Hotspot.Media inputMedia, bool expected)>> IsCompatibleDataTestCases =>
            new[]
            {
                ("Compatible", Array.Empty<string>(), Array.Empty<string>(), true),
                ("IncompatibleImages", new[] { "image_0.png" }, Array.Empty<string>(), false),
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
            // var (inputMedia, expected) = testCase;
            // var factory = new DescriptionViewModel.Factory();
            //
            // Assert.That(factory.IsCompatibleData(inputMedia), Is.EqualTo(expected));
        }

        [Test]
        [TestCaseSource(nameof(IsCompatibleDataTestCases))]
        public void CreateLayoutTest((Hotspot.Media inputMedia, bool expected) testCase)
        {
            CreateLayoutTestBody(
                testCase,
                (descriptionViewModel, inputMedia) => Assert.Multiple(() =>
                {
                    Assert.That(descriptionViewModel.Title, Is.EqualTo(inputMedia.Title));
                    Assert.That(descriptionViewModel.Description, Is.EqualTo(inputMedia.Description));
                })
            );
            // var (inputMedia, expected) = testCase;
            // var vmProvider = new MockViewModelProvider();
            // var factory = new DescriptionViewModel.Factory();
            //
            // if (!expected)
            // {
            //     Assert.That(() => factory.CreateLayout(vmProvider, inputMedia), Throws.ArgumentException);
            //     return;
            // }
            //
            // var layout = factory.CreateLayout(vmProvider, inputMedia);
            // Assert.That(layout, Is.TypeOf<DescriptionViewModel>());
            //
            // var descriptionViewModel = (DescriptionViewModel)layout;
            // Assert.Multiple(() =>
            // {
            //     Assert.That(descriptionViewModel.Title, Is.EqualTo(inputMedia.Title));
            //     Assert.That(descriptionViewModel.Description, Is.EqualTo(inputMedia.Description));
            // });
        }
    }
}
