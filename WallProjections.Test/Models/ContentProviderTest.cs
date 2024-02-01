using System.Collections.Immutable;
using System.IO.Compression;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks.Models;
using static WallProjections.Test.TestExtensions;

namespace WallProjections.Test.Models;

[TestFixture]
public class ContentProviderTest
{
    private const string ValidConfigPath = "Valid";
    private const string InvalidConfigPath = "Invalid";

    private static string TestZipPath =>
        Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test.zip");

    private static string TestInvalidZipPath =>
        Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_invalid.zip");

    private string _configPath = null!;

    private readonly IConfig _mockConfig = new Config(new List<Hotspot>
    {
        new(
            0,
            new Coord(0,0,0),
            "text_0.txt",
            ImmutableList<string>.Empty,
            ImmutableList<string>.Empty
            ),
        new(
            1,
            new Coord(0,0,0),
            "text_1.txt",
            new List<string>{ "image_1_0.png" }.ToImmutableList(),
            new List<string>{ "video_1_0.mp4" }.ToImmutableList()
            ),
        new(
            2,
            new Coord(0,0,0),
            "text_2.txt",
            new List<string>{ "image_2_0.jpg", "image_2_1.jpeg" }.ToImmutableList(),
            new List<string>{ "video_2_0.mkv", "video_2_1.mov" }.ToImmutableList()
            )
    });

    // Enumerable.Range(0, 5).Select(id => new Hotspot(
    //     id,
    // new Coord(1, 1, 1),
    //     "text_0.txt",
    // ImmutableList.Create("image_1_0.png"),
    // ImmutableList.Create("video_1_0.mp4")
    // ))

    private string MediaPath => Path.Combine(_configPath, ValidConfigPath);
    private string InvalidMediaPath => Path.Combine(_configPath, InvalidConfigPath);

    private string GetFullPath(string file) => Path.Combine(MediaPath, file);

    private string GetDescription(string descFile) => File.ReadAllText(GetFullPath(descFile));

    private static IEnumerable<TestCaseData<(int, string, string[], string[])>> TestCases()
    {
        yield return MakeTestData(
            (0, "text_0.txt", Array.Empty<string>(), Array.Empty<string>()),
            "TextOnly"
        );
        yield return MakeTestData(
            (1, "text_1.txt", new[] { "image_1_0.png" }, new[] { "video_1_0.mp4" }),
            "FilenamesAreIDs"
        );
        yield return MakeTestData(
            (2, "text_2.txt", new[] { "image_2_0.jpg", "image_2_1.jpeg" }, new[] { "video_2_0.mkv", "video_2_1.mov" }),
            "Multiple Files"
        );
    }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        // Create a temporary directory for the test
        _configPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var valid = Path.Combine(_configPath, ValidConfigPath);
        Directory.CreateDirectory(valid);
        ZipFile.ExtractToDirectory(TestZipPath, valid);

        var invalid = Path.Combine(_configPath, InvalidConfigPath);
        Directory.CreateDirectory(invalid);
        ZipFile.ExtractToDirectory(TestInvalidZipPath, invalid);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        // Clean up the temporary directory
        Directory.Delete(_configPath, true);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void GetMediaTest((int, string, string[], string[]) testCase)
    {
        var (id, descPath, imagePaths, videoPaths) = testCase;
        var provider = new ContentProvider(_mockConfig, Path.Combine(_configPath, ValidConfigPath));

        var media = provider.GetMedia(id);
        var expectedDescription = GetDescription(descPath);
        Assert.Multiple(() =>
        {
            Assert.That(media.Description, Is.EqualTo(expectedDescription));
            Assert.That(
                imagePaths.Select(path => GetFullPath(path)),
                media.ImagePath is not null ? Has.Member(GetFullPath(media.ImagePath)) : Is.Empty
            );
            Assert.That(
                videoPaths.Select(path => GetFullPath(path)),
                media.VideoPath is not null ? Has.Member(GetFullPath(media.VideoPath)) : Is.Empty
            );
        });
    }

    [Test]
    public void GetMediaNoHotspotTest()
    {
        var provider = new ContentProvider(_mockConfig, ValidConfigPath);

        Assert.Throws<IConfig.HotspotNotFoundException>(() => provider.GetMedia(-1));
    }

    [Test]
    public void GetMediaNoDescriptionTest()
    {
        var provider = new ContentProvider(_mockConfig, ValidConfigPath);

        Assert.Throws<FileNotFoundException>(() => provider.GetMedia(1));
    }
}
