using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using System.Diagnostics.CodeAnalysis;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.Display;
#if DEBUGSKIPPYTHON
using Avalonia.Data;
using WallProjections.Helper;
#endif

namespace WallProjections.Views;

public partial class DisplayWindow : ReactiveWindow<IDisplayViewModel>
{
    
    public DisplayWindow()
    {
        InitializeComponent();
    }

    // ReSharper disable UnusedParameter.Local

    /// <summary>
    /// Handles key presses:
    /// <ul>
    ///     <li><b>F11</b>: Toggles fullscreen</li>
    ///     <li><b>Escape</b>: Closes the display</li>
    ///     <li><b>E</b>: Opens the editor</li>
    /// </ul>
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments containing the key that was pressed.</param>
    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        // Toggle fullscreen
        if (e.Key == Key.F11)
        {
            WindowState = WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;
            return;
        }

        if (DataContext is not IDisplayViewModel viewModel) return;

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (e.Key)
        {
            // Close the display
            case Key.Escape:
            {
                viewModel.CloseDisplay();
                return;
            }

            // Open the editor
            case Key.E:
            {
                viewModel.OpenEditor();
                return;
            }

            default:
            {
#if DEBUGSKIPPYTHON
                MockPythonInput(e.Key);
#endif
                base.OnKeyDown(e);
                break;
            }
        }
    }

    /// <summary>
    /// Intercepts the window closing event to allow the ViewModel to handle it.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments containing the reason for the window closing.</param>
    /// <remarks>
    /// The window should never be closed using <see cref="Window.Close()" />; instead, the ViewModel's
    /// <see cref="IDisplayViewModel.CloseDisplay" /> method should be called.
    /// </remarks>
    internal void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is not IDisplayViewModel viewModel) return;

        // The window is being closed programmatically only by ViewModel's Navigator
        if (e.IsProgrammatic) return;

        e.Cancel = true;

        viewModel.CloseDisplay();
    }

    // ReSharper restore UnusedParameter.Local

#if DEBUGSKIPPYTHON
    /// <summary>
    /// Mocks the input from the Python script for testing purposes.
    /// </summary>
    /// <param name="key"></param>
    [ExcludeFromCodeCoverage]
    private static void MockPythonInput(Key key)
    {
        var keyVal = key switch
        {
            Key.D0 => new Optional<int>(0),
            Key.D1 => new Optional<int>(1),
            Key.D2 => new Optional<int>(2),
            Key.D3 => new Optional<int>(3),
            Key.D4 => new Optional<int>(4),
            Key.D5 => new Optional<int>(5),
            Key.D6 => new Optional<int>(6),
            Key.D7 => new Optional<int>(7),
            Key.D8 => new Optional<int>(8),
            Key.D9 => new Optional<int>(9),
            _ => new Optional<int>()
        };

        if (!keyVal.HasValue) return;

        PythonHandler.Instance.OnPressDetected(keyVal.Value);
    }
#endif
}
