using System;
using System.Diagnostics.CodeAnalysis;
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
    /// A routed event that is raised when the user confirms the action.
    /// </summary>
    private static readonly RoutedEvent<RoutedEventArgs> ConfirmEvent =
        RoutedEvent.Register<ConfirmationDialog, RoutedEventArgs>(nameof(Confirm), RoutingStrategies.Bubble);

    /// <summary>
    /// A routed event that is raised when the user refuses the action.
    /// </summary>
    private static readonly RoutedEvent<RoutedEventArgs> RefuseEvent =
        RoutedEvent.Register<ConfirmationDialog, RoutedEventArgs>(nameof(Refuse), RoutingStrategies.Bubble);

    /// <summary>
    /// A routed event that is raised when the user cancels the action.
    /// </summary>
    private static readonly RoutedEvent<RoutedEventArgs> CancelEvent =
        RoutedEvent.Register<ConfirmationDialog, RoutedEventArgs>(nameof(Cancel), RoutingStrategies.Bubble);

    /// <summary>
    /// An event that is raised when the user confirms the action.
    /// The dialog is closed after the event is raised.
    /// </summary>
    public event EventHandler<RoutedEventArgs> Confirm
    {
        add => AddHandler(ConfirmEvent, value);
        remove => RemoveHandler(ConfirmEvent, value);
    }

    /// <summary>
    /// An event that is raised when the user refuses the action.
    /// </summary>
    public event EventHandler<RoutedEventArgs> Refuse
    {
        add => AddHandler(RefuseEvent, value);
        remove => RemoveHandler(RefuseEvent, value);
    }

    /// <summary>
    /// An event that is raised when the user cancels the action.
    /// The dialog is closed after the event is raised.
    /// </summary>
    public event EventHandler<RoutedEventArgs> Cancel
    {
        add => AddHandler(CancelEvent, value);
        remove => RemoveHandler(CancelEvent, value);
    }

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
    /// A callback for when the user confirms the action. Closes the dialog.
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

        RaiseEvent(new RoutedEventArgs(ConfirmEvent, this));
        Close();
    }

    /// <summary>
    /// A callback for when the user refuses the action. Closes the dialog.
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

        RaiseEvent(new RoutedEventArgs(RefuseEvent, this));
        Close();
    }

    /// <summary>
    /// A callback for when the user cancels the action. Closes the dialog.
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

        RaiseEvent(new RoutedEventArgs(CancelEvent, this));
        Close();
    }

    /// <summary>
    /// A callback for when the dialog is closed. Raises the <see cref="Cancel" /> event
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

        RaiseEvent(new RoutedEventArgs(CancelEvent, this));
    }

    // ReSharper restore UnusedParameter.Local

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
