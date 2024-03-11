using System;
using Avalonia.Controls;
using Avalonia.Input;
#if !RELEASE
using Avalonia;
#endif

namespace WallProjections.Views.Display;

public partial class HotspotDisplayWindow : Window
{
    public HotspotDisplayWindow()
    {
        InitializeComponent();
#if !RELEASE
        this.AttachDevTools();
#endif
    }

    // ReSharper disable UnusedParameter.Local

    /// <summary>
    /// Handles key presses:
    /// <ul>
    ///     <li><b>F11</b>: Toggles fullscreen</li>
    /// </ul>
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments containing the key that was pressed.</param>
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F11)
            WindowState = WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;
    }

    /// <summary>
    /// Disposes the viewmodel when the window is closed.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is IDisposable disposable)
            disposable.Dispose();
    }

    // ReSharper restore UnusedParameter.Local
}
