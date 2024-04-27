using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
#if !RELEASE
using Avalonia;
#endif

namespace WallProjections.Views;

/// <summary>
/// A dialog shown when the application is closing and performs a cleanup.
/// </summary>
public partial class AppClosingDialog : Window
{
    /// <summary>
    /// The cleanup to perform while this dialog is open.
    /// Once it's done, the dialog will close.
    /// </summary>
    private readonly Action _cleanup;

    /// <summary>
    /// Creates a new <see cref="AppClosingDialog"/>.
    /// </summary>
    /// <param name="cleanup">The cleanup to perform while this dialog is open.</param>
    public AppClosingDialog(Action cleanup)
    {
        _cleanup = cleanup;
        InitializeComponent();
#if !RELEASE
        this.AttachDevTools();
#endif
    }

    // ReSharper disable UnusedParameter.Local

    /// <inheritdoc cref="_cleanup" />
    private async void Cleanup(object? sender, RoutedEventArgs e)
    {
        // Let the window open up properly
        await Task.Delay(200);
        Dispatcher.UIThread.RunJobs();

        _cleanup();
        Close();
    }

    /// <summary>
    /// Prevents the dialog from closing unless the window is closing programmatically.
    /// </summary>
    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (!e.IsProgrammatic)
            e.Cancel = true;
    }

    // ReSharper restore UnusedParameter.Local

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// This constructor should not be called. It is only here to suppress an Avalonia warning.
    /// Use <see cref="AppClosingDialog" /> instead.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
    [ExcludeFromCodeCoverage]
    [Obsolete("This constructor should not be called. See the documentation for more information.", true)]
    public AppClosingDialog() => throw new InvalidOperationException(
        "This constructor should not be called. See the documentation for more information."
    );
}
