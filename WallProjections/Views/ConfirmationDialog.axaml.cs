using System;
using System.Diagnostics.CodeAnalysis;
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
    /// A routed event that is raised when the user confirms the action.
    /// </summary>
    private static readonly RoutedEvent<RoutedEventArgs> ConfirmEvent =
        RoutedEvent.Register<ConfirmationDialog, RoutedEventArgs>(nameof(Confirm), RoutingStrategies.Bubble);

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
    /// <param name="cancelButtonText">The text for the cancel button.</param>
    public ConfirmationDialog(
        string title,
        Uri iconPath,
        string message,
        string confirmButtonText,
        string cancelButtonText = "Cancel"
    )
    {
        InitializeComponent();
        DataContext = new ConfirmationDialogViewModel(title, iconPath, message, confirmButtonText, cancelButtonText);
    }

    // ReSharper disable UnusedParameter.Local

    /// <summary>
    /// A callback for when the user confirms the action. Closes the dialog.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private void Confirm_OnClick(object? sender, RoutedEventArgs? e)
    {
        RaiseEvent(new RoutedEventArgs(ConfirmEvent, this));
        Close();
    }

    /// <summary>
    /// A callback for when the user cancels the action. Closes the dialog.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private void Cancel_OnClick(object? sender, RoutedEventArgs? e)
    {
        RaiseEvent(new RoutedEventArgs(CancelEvent, this));
        Close();
    }

    // ReSharper restore UnusedParameter.Local

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// This constructor should not be called. It is only here to suppress an Avalonia warning.
    /// Use <see cref="ConfirmationDialog(string, Uri, string, string, string)" /> instead.
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
    public string CancelButtonText { get; }

    public ConfirmationDialogViewModel(
        string title,
        Uri iconPath,
        string message,
        string confirmButtonText,
        string cancelButtonText
    )
    {
        Title = title;
        Icon = new WindowIcon(new Bitmap(AssetLoader.Open(iconPath)));
        Message = message;
        ConfirmButtonText = confirmButtonText;
        CancelButtonText = cancelButtonText;
    }
}
