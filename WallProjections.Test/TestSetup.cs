using WallProjections.Models;
using WallProjections.ViewModels;

namespace WallProjections.Test;

[SetUpFixture]
public class TestSetup
{
    [OneTimeTearDown]
    public void GlobalTearDown()
    {
        // Dispose of the global singletons after all tests have run
        ContentCache.Instance.Dispose();
        ViewModelProvider.Instance.Dispose();
    }
}
