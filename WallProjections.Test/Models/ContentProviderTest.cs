using System.IO.Compression;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks.Models;

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
    private readonly IConfig _mockConfig = new Config(Enumerable.Range(0, 5).Select(id => new Hotspot(id)));
    private readonly List<Hotspot.Media> _cacheMockMedia = new();

    private string MediaPath => Path.Combine(_configPath, ValidConfigPath, "Media");
    private string InvalidMediaPath => Path.Combine(_configPath, InvalidConfigPath, "Media");

    private string GetFullPath(int id, string file) => Path.Combine(MediaPath, id.ToString(), file);

    private string GetDescription(int id, string descFile) => File.ReadAllText(GetFullPath(id, descFile));

    private static IEnumerable<(int, string, IReadOnlyList<string>?, IReadOnlyList<string>?)> TestCases()
    {
        yield return (0, "0.txt", Array.Empty<string>(), Array.Empty<string>());
        yield return (1, "1.txt", new[] { "1.png" }, new[] { "1.mp4" });
        yield return (2, "test2.txt", new[] { "1_2.jpg", "random.jpeg" }, new[] { "2.mkv", "2_1.mov" });
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
    public void GetMediaTest((int, string, IReadOnlyList<string>, IReadOnlyList<string>) testCase)
    {
        var (id, descPath, imagePaths, videoPaths) = testCase;
        var cache = new MockContentCache(_mockConfig, MediaPath, _cacheMockMedia);
        var provider = new ContentProvider(cache, _mockConfig);

        var media = provider.GetMedia(id);
        var expectedDescription = GetDescription(id, descPath);
        Assert.Multiple(() =>
        {
            Assert.That(media.Description, Is.EqualTo(expectedDescription));
            Assert.That(
                imagePaths.Select(path => GetFullPath(id, path)),
                media.ImagePath is not null ? Has.Member(GetFullPath(id, media.ImagePath)) : Is.Empty
            );
            Assert.That(
                videoPaths.Select(path => GetFullPath(id, path)),
                media.VideoPath is not null ? Has.Member(GetFullPath(id, media.VideoPath)) : Is.Empty
            );
        });
    }

    [Test]
    public void GetMediaNoHotspotTest()
    {
        var cache = new MockContentCache(_mockConfig, MediaPath, _cacheMockMedia);
        var provider = new ContentProvider(cache, _mockConfig);

        Assert.Throws<IConfig.HotspotNotFoundException>(() => provider.GetMedia(-1));
    }

    [Test]
    public void GetMediaNoDescriptionTest()
    {
        var cache = new MockContentCache(_mockConfig, InvalidMediaPath, _cacheMockMedia);
        var provider = new ContentProvider(cache, _mockConfig);

        Assert.Throws<FileNotFoundException>(() => provider.GetMedia(1));
    }
}
