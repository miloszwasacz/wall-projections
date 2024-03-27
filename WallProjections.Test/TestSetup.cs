using WallProjections.Helper;
using WallProjections.Helper.Interfaces;
using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks.Helper;

namespace WallProjections.Test;

[SetUpFixture]
public class TestSetup
{
    /// <summary>
    /// A mock of the Python runtime supplied to the <see cref="PythonHandler.Instance">Python handler</see>
    /// at <see cref="PythonHandler.Initialize">initialization</see>
    /// </summary>
    public static MockPythonProxy PythonRuntime { get; } = new();

    /// <summary>
    /// The global instance of the <see cref="PythonHandler.Instance">Python handler</see>
    /// </summary>
    private static IPythonHandler? _pythonHandler;

    [OneTimeSetUp]
    public void GlobalSetup()
    {
        // Set up the global singletons before any tests run
        _pythonHandler = PythonHandler.Initialize(PythonRuntime);
    }

    [OneTimeTearDown]
    public void GlobalTearDown()
    {
        // Dispose of the global singletons after all tests have run
        IFileHandler.DeleteConfigFolder();
        _pythonHandler?.Dispose();
    }
}

public static class TestExtensions
{
    /// <summary>
    /// A generic wrapper for <see cref="TestCaseData" />
    /// </summary>
    public class TestCaseData<T> : TestCaseData
    {
        /// <summary>
        /// A test case data object with a name
        /// </summary>
        /// <param name="obj">The test case data object</param>
        /// <param name="testName">The name of the test</param>
        public TestCaseData(T obj, string testName) : base(obj)
        {
            SetName(testName);
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="TestCaseData{T}" /> with a name
    /// </summary>
    /// <param name="obj">The test case data object</param>
    /// <param name="name">The name of the test</param>
    /// <typeparam name="T">The type of test case data object</typeparam>
    /// <returns>A new instance of <see cref="TestCaseData{T}(T, string)" /></returns>
    public static TestCaseData<T> MakeTestData<T>(T obj, string name) => new(obj, name);
}
