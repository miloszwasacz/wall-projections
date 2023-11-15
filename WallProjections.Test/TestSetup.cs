using WallProjections.ViewModels;

namespace WallProjections.Test;

[SetUpFixture]
public class TestSetup
{
    [OneTimeTearDown]
    public void GlobalTearDown()
    {
        // Dispose of the global ViewModelProvider instance after all tests have run
        ViewModelProvider.Instance.Dispose();
    }
}
