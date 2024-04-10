using Avalonia;
using Avalonia.Headless;
using Avalonia.ReactiveUI;
using WallProjections.Test;
using WallProjections.Test.Mocks;
using WallProjections.Test.Mocks.Helper;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace WallProjections.Test;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure(() =>
        {
            var pythonHandler = new MockPythonHandler();
            var loggerFactory = new MockLoggerFactory();
            return new App(pythonHandler, loggerFactory);
        })
        .UseHeadless(new AvaloniaHeadlessPlatformOptions())
        .UseReactiveUI();
}
