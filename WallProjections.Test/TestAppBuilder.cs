using Avalonia;
using Avalonia.Headless;
using Avalonia.ReactiveUI;
using WallProjections.Test;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace WallProjections.Test;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions())
        .UseReactiveUI();
}
