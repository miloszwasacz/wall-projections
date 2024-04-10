using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace WallProjections.Views;

/// <summary>
/// A generic confirmation dialog.
/// </summary>
public partial class ConfirmationDialog : Window
{
    /// <summary>
    /// Whether the dialog has been handled (confirmed or cancelled).
    /// </summary>
    private bool _handled;

    /// <summary>
    /// The result of the dialog.
    /// </summary>
    /// <seealso cref="ShowDialog" />
    private Result _result = Result.Cancelled;

    /// <summary>
    /// Creates a new <see cref="ConfirmationDialog"/>.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="iconPath">A <see cref="Uri" /> to the icon of the dialog.</param>
    /// <param name="message">The message displayed by the dialog.</param>
    /// <param name="confirmButtonText">The text for the confirm button.</param>
    /// <param name="refuseButtonText">The text for the refuse button. If null or empty, the button is hidden.</param>
    /// <param name="cancelButtonText">The text for the cancel button.</param>
    public ConfirmationDialog(
        string title,
        Uri iconPath,
        string message,
        string confirmButtonText,
        string? refuseButtonText = null,
        string cancelButtonText = "Cancel"
    )
    {
        InitializeComponent();
        this.AttachDevTools();
        DataContext = new ConfirmationDialogViewModel(
            title,
            iconPath,
            message,
            confirmButtonText,
            refuseButtonText,
            cancelButtonText
        );
    }

    // ReSharper disable UnusedParameter.Local

    /// <summary>
    /// A callback for when the user confirms the action.
    /// Sets <see cref="_result" /> to <see cref="Result.Confirmed" /> and closes the dialog.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private void Confirm_OnClick(object? sender, RoutedEventArgs? e)
    {
        lock (this)
        {
            if (_handled) return;

            _handled = true;
        }

        _result = Result.Confirmed;
        Close();
    }

    /// <summary>
    /// A callback for when the user refuses the action.
    /// Sets <see cref="_result" /> to <see cref="Result.Refused" /> and closes the dialog.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private void Refuse_OnClick(object? sender, RoutedEventArgs e)
    {
        lock (this)
        {
            if (_handled) return;

            _handled = true;
        }

        _result = Result.Refused;
        Close();
    }

    /// <summary>
    /// A callback for when the user cancels the action.
    /// Sets <see cref="_result" /> to <see cref="Result.Cancelled" /> and closes the dialog.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private void Cancel_OnClick(object? sender, RoutedEventArgs? e)
    {
        lock (this)
        {
            if (_handled) return;

            _handled = true;
        }

        _result = Result.Cancelled;
        Close();
    }

    /// <summary>
    /// A callback for when the dialog is closed. Sets <see cref="_result" /> to <see cref="Result.Cancelled" />
    /// if it hasn't already been <see cref="_handled">handled</see>.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private void Dialog_OnClosed(object? sender, EventArgs e)
    {
        lock (this)
        {
            if (_handled) return;

            _handled = true;
        }

        _result = Result.Cancelled;
    }

    // ReSharper restore UnusedParameter.Local

    /// <inheritdoc cref="Window.ShowDialog" />
    /// <returns>A task that can be used to retrieve the result of the dialog when it closes.</returns>
    public new async Task<Result> ShowDialog(Window owner)
    {
        await base.ShowDialog(owner);
        return _result;
    }

    /// <summary>
    /// The possible results of the dialog.
    /// </summary>
    public enum Result
    {
        Cancelled,
        Confirmed,
        Refused
    }

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// This method should not be called. Use <see cref="ShowDialog(Window)" /> instead.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
    [Obsolete("This method should not be called. See the documentation for more information.", true)]
    public new Task<TResult> ShowDialog<TResult>(Window _) => throw new InvalidOperationException(
        "This method should not be called. See the documentation for more information."
    );

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// This constructor should not be called. It is only here to suppress an Avalonia warning.
    /// Use <see cref="ConfirmationDialog(string, Uri, string, string, string?, string)" /> instead.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
    [ExcludeFromCodeCoverage]
    [Obsolete("This constructor should not be called. See the documentation for more information.", true)]
    public ConfirmationDialog()
    {
        throw new InvalidOperationException(
            "This constructor should not be called. See the documentation for more information.");
    }
}

/// <summary>
/// A viewmodel for <see cref="ConfirmationDialog" />.
/// </summary>
internal class ConfirmationDialogViewModel
{
    public string Title { get; }
    public WindowIcon Icon { get; }
    public string Message { get; }
    public string ConfirmButtonText { get; }
    public string RefuseButtonText { get; }
    public string CancelButtonText { get; }
    public bool HasRefuseButton { get; }

    public ConfirmationDialogViewModel(
        string title,
        Uri iconPath,
        string message,
        string confirmButtonText,
        string? refuseButtonText,
        string cancelButtonText
    )
    {
        Title = title;
        Icon = new WindowIcon(new Bitmap(AssetLoader.Open(iconPath)));
        Message = message;
        ConfirmButtonText = confirmButtonText;
        RefuseButtonText = refuseButtonText ?? "";
        CancelButtonText = cancelButtonText;
        HasRefuseButton = !string.IsNullOrEmpty(RefuseButtonText);
    }
}
