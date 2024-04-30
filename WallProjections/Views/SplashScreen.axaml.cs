using Avalonia.Controls;
#if !RELEASE
using Avalonia;
#endif

namespace WallProjections.Views;

public partial class SplashScreen : Window
{
    public SplashScreen()
    {
        InitializeComponent();
#if !RELEASE
        this.AttachDevTools();
#endif
    }

    // ReSharper disable once UnusedParameter.Local
    /// <summary>
    /// Prevent the window from closing unless it's programmatic.
    /// </summary>
    /// <param name="sender">The sender of the event (unused)</param>
    /// <param name="e">The event arguments</param>
    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (!e.IsProgrammatic)
            e.Cancel = true;
    }
}
