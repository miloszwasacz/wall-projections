using System.Collections.Immutable;
using System.IO.Compression;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
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

    private IConfig _mockValidConfig = null!;
    private IConfig _mockInvalidConfig = null;

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

        _mockValidConfig = new Config(new List<Hotspot>
        {
            new(
                0,
                new Coord(0,0,0),
                "text_0.txt",
                ImmutableList<string>.Empty,
                ImmutableList<string>.Empty,
                Path.Combine(_configPath, ValidConfigPath)
            ),
            new(
                1,
                new Coord(0,0,0),
                "text_1.txt",
                new List<string>{ "image_1_0.png" }.ToImmutableList(),
                new List<string>{ "video_1_0.mp4" }.ToImmutableList(),
                Path.Combine(_configPath, ValidConfigPath)
            ),
            new(
                2,
                new Coord(0,0,0),
                "text_2.txt",
                new List<string>{ "image_2_0.jpg", "image_2_1.jpeg" }.ToImmutableList(),
                new List<string>{ "video_2_0.mkv", "video_2_1.mov" }.ToImmutableList(),
                Path.Combine(_configPath, ValidConfigPath)
            )
        });

        _mockInvalidConfig = new Config(new List<Hotspot>
        {
            new(
                0,
                new Coord(0,0,0),
                "text_0.txt",
                ImmutableList<string>.Empty,
                ImmutableList<string>.Empty,
                Path.Combine(_configPath, InvalidConfigPath)
            ),
            new(
                1,
                new Coord(0,0,0),
                "text_1.txt",
                new List<string>{ "image_1_0.png" }.ToImmutableList(),
                ImmutableList<string>.Empty,
                Path.Combine(_configPath, InvalidConfigPath)
            )
        });
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
        var provider = new ContentProvider(_mockValidConfig);

        var media = provider.GetMedia(id);
        var expectedDescription = GetDescription(descPath);
        Assert.Multiple(() =>
        {
            Assert.That(media.Description, Is.EqualTo(expectedDescription));
            Assert.That(
                imagePaths.Select(GetFullPath).ToImmutableList(), Is.EqualTo(media.ImagePaths)
            );
            Assert.That(
                videoPaths.Select(GetFullPath).ToImmutableList(), Is.EqualTo(media.VideoPaths)
            );
        });
    }

    [Test]
    public void GetMediaNoHotspotTest()
    {
        var provider = new ContentProvider(_mockInvalidConfig);

        Assert.Throws<IConfig.HotspotNotFoundException>(() => provider.GetMedia(-1));
    }

    [Test]
    public void GetMediaNoDescriptionTest()
    {
        var provider = new ContentProvider(_mockInvalidConfig);

        Assert.Throws<FileNotFoundException>(() => provider.GetMedia(1));
    }
}
