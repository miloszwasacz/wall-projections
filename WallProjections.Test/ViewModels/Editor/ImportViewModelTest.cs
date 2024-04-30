using WallProjections.Test.Mocks;
using WallProjections.Test.Mocks.ViewModels.Editor;
using WallProjections.ViewModels.Editor;
using WallProjections.ViewModels.Interfaces.Editor;
using static WallProjections.Test.TestExtensions;

namespace WallProjections.Test.ViewModels.Editor;

[TestFixture]
public class ImportViewModelTest
{
    private static readonly string TestAssets =
        Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "ImportViewModelTestCases");

    private static IEnumerable<TestCaseData<(string, string, string)>> ImportFromFileTestCases
    {
        get
        {
            var files = Directory.GetFiles(TestAssets).Where(file => !file.EndsWith("empty.txt")).ToArray();
            if (files.Length == 0)
                throw new FileNotFoundException("No test files found in " + TestAssets);

            foreach (var file in files)
            {
                var testName = Path.GetFileNameWithoutExtension(file);

                var lines = File.ReadAllLines(file);
                var title = lines[0].Trim();
                var description = lines.Length > 1
                    ? string.Join("\n", lines[1..]).Trim()
                    : "";

                yield return MakeTestData((file, title, description), testName);
            }
        }
    }

    private static string GetTestFile(string fileName) => Path.Combine(TestAssets, fileName);

    /// <summary>
    /// Creates a new <see cref="ImportViewModel" /> with a <see cref="MockDescriptionEditorViewModel">mock</see>
    /// parent <see cref="ImportViewModel.DescriptionEditor" />.
    /// </summary>
    /// <returns></returns>
    private static ImportViewModel CreateViewModel()
    {
        var descriptionEditor = new MockDescriptionEditorViewModel(
            "Title",
            "Description",
            descVm => new ImportViewModel(descVm, new MockLoggerFactory())
        );
        var importVm = descriptionEditor.Importer as ImportViewModel;
        return importVm!;
    }

    [Test]
    public void ConstructorTest()
    {
        var importViewModel = CreateViewModel();
        Assert.That(importViewModel.DescriptionEditor, Is.InstanceOf<IDescriptionEditorViewModel>());
    }

    [Test]
    [TestCase("", "", ImportWarningLevel.None, TestName = "Empty")]
    [TestCase("    \n", "\t\n", ImportWarningLevel.None, TestName = "Whitespace")]
    [TestCase("Title", " \r ", ImportWarningLevel.Title, TestName = "TitleOnly")]
    [TestCase("", " D e s c r i p t i o n ", ImportWarningLevel.Description, TestName = "DescriptionOnly")]
    [TestCase("  Title", "\tDescription\r\nTest\r\n", ImportWarningLevel.Both, TestName = "TitleAndDescription")]
    public void IsImportSafeTest(string title, string description, ImportWarningLevel expected)
    {
        var importViewModel = CreateViewModel();
        var descriptionEditor = importViewModel.DescriptionEditor;

        descriptionEditor.Title = title;
        descriptionEditor.Description = description;
        Assert.That(importViewModel.IsImportSafe(), Is.EqualTo(expected));
    }

    [Test]
    [TestCaseSource(nameof(ImportFromFileTestCases))]
    public void ImportFromFileTest((string, string, string) testCase)
    {
        var (path, expectedTitle, expectedDescription) = testCase;
        var importViewModel = CreateViewModel();
        var descriptionEditor = importViewModel.DescriptionEditor;

        var result = importViewModel.ImportFromFile(path);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(descriptionEditor.Title, Is.EqualTo(expectedTitle));
            Assert.That(descriptionEditor.Description, Is.EqualTo(expectedDescription));
        });
    }

    [Test]
    [TestCase("empty.txt", true, TestName = "Empty")]
    [TestCase("nonexistent.txt", false, TestName = "Nonexistent")]
    public void ImportFromFileInvalidOrEmptyTest(string file, bool successful)
    {
        var importViewModel = CreateViewModel();
        var descriptionEditor = importViewModel.DescriptionEditor;
        var title = descriptionEditor.Title;
        var description = descriptionEditor.Description;

        var path = GetTestFile(file);
        var result = importViewModel.ImportFromFile(path);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(successful));
            Assert.That(descriptionEditor.Title, Is.EqualTo(title));
            Assert.That(descriptionEditor.Description, Is.EqualTo(description));
        });
    }

    [TestFixture]
    public class ImportWarningLevelExtensionsTest
    {
        [Test]
        public void ToWarningTextTest()
        {
            Assert.Multiple(() =>
            {
                Assert.That(ImportWarningLevel.None.ToWarningText(), Is.Empty);
                Assert.That(ImportWarningLevel.Title.ToWarningText(), Is.Not.Empty);
                Assert.That(ImportWarningLevel.Description.ToWarningText(), Is.Not.Empty);
                Assert.That(ImportWarningLevel.Both.ToWarningText(), Is.Not.Empty);
                Assert.That(() => ((ImportWarningLevel)4).ToWarningText(),
                    Throws.TypeOf<ArgumentOutOfRangeException>());
            });
        }
    }
}
