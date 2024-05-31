using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using WallProjections.ViewModels.Interfaces.Display;
#if DEBUGSKIPPYTHON
using System.Diagnostics.CodeAnalysis;
using Avalonia.Data;
#endif
#if !RELEASE
using Avalonia;
#endif

namespace WallProjections.Views.Display;

public partial class DisplayWindow : ReactiveWindow<IDisplayViewModel>
{
#if DEBUGSKIPPYTHON
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Whether the application is running in a CI environment.
    /// </summary>
    public bool IsCI { get; init; }
#endif

    public DisplayWindow()
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
    ///     <li><b>F</b>: Toggles fullscreen</li>
    ///     <li><b>Escape</b>: Closes the display</li>
    ///     <li><b>E</b>: Opens the editor</li>
    /// </ul>
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments containing the key that was pressed.</param>
    internal async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        // Ignore handled events and ones with key modifiers
        if (e.Handled || e.KeyModifiers != KeyModifiers.None) return;

        // Toggle fullscreen
        if (e.Key == Key.F)
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
                await viewModel.OpenEditor();
                return;
            }

            default:
            {
#if DEBUGSKIPPYTHON
                if (!IsCI)
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
    [ExcludeFromCodeCoverage(Justification = "Mock keyboard input for manual testing")]
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
        if (keyVal.HasValue)
            (Application.Current as App)?.PythonHandler.OnHotspotPressed(keyVal.Value);


        var keyValUnpressed = key switch
        {
            Key.NumPad0 => new Optional<int>(0),
            Key.NumPad1 => new Optional<int>(1),
            Key.NumPad2 => new Optional<int>(2),
            Key.NumPad3 => new Optional<int>(3),
            Key.NumPad4 => new Optional<int>(4),
            Key.NumPad5 => new Optional<int>(5),
            Key.NumPad6 => new Optional<int>(6),
            Key.NumPad7 => new Optional<int>(7),
            Key.NumPad8 => new Optional<int>(8),
            Key.NumPad9 => new Optional<int>(9),
            _ => new Optional<int>()
        };
        if (keyValUnpressed.HasValue)
            (Application.Current as App)?.PythonHandler.OnHotspotUnpressed(keyValUnpressed.Value);
    }
#endif
}
