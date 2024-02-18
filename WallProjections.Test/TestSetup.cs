using WallProjections.Models.Interfaces;

namespace WallProjections.Test;

[SetUpFixture]
public class TestSetup
{
    [OneTimeTearDown]
    public void GlobalTearDown()
    {
        // Dispose of the global singletons after all tests have run
        IFileHandler.DeleteConfigFolder();
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
