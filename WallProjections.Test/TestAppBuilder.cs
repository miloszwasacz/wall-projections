using Avalonia;
using Avalonia.Headless;
using Avalonia.ReactiveUI;
using WallProjections.Test;
using WallProjections.Test.Mocks;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace WallProjections.Test;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure(() => new App(new MockLoggerFactory()))
            .UseHeadless(new AvaloniaHeadlessPlatformOptions())
            .UseReactiveUI();
}
