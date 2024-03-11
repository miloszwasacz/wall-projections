using System;
using Avalonia.Controls;
using Avalonia.Input;
#if !RELEASE
using Avalonia;
#endif

namespace WallProjections.Views;

public partial class SecondaryWindow : Window
{
    public SecondaryWindow()
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
    /// Disposes the viewmodel when the window is being closed programmatically;
    /// otherwise, cancels the close event.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        // Unless the window is being closed programmatically (i.e. by the Navigator), cancel the close event.
        if (!e.IsProgrammatic)
        {
            e.Cancel = true;
            return;
        }

        if (DataContext is IDisposable disposable)
            disposable.Dispose();
    }

    // ReSharper restore UnusedParameter.Local
}
