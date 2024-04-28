using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace WallProjections.Views;

/// <summary>
/// A generic dialog window which yields a result when closed.
/// </summary>
public abstract class ResultDialog : Window
{
    /// <summary>
    /// Whether the dialog has been handled (confirmed or cancelled).
    /// </summary>
    protected bool Handled;

    /// <summary>
    /// The result of the dialog.
    /// </summary>
    /// <seealso cref="ShowDialog" />
    protected Result DialogResult = Result.Cancelled;

    /// <summary>
    /// An action to be called when the dialog is closed.
    /// </summary>
    /// <seealso cref="ShowStandaloneDialog" />
    protected Action<Result>? OnClose;

    // ReSharper disable UnusedParameter.Local

    /// <summary>
    /// A callback for when the user confirms the action.
    /// Sets <see cref="DialogResult" /> to <see cref="Result.Confirmed" /> and closes the dialog.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    protected virtual void Confirm_OnClick(object? sender, RoutedEventArgs? e)
    {
        lock (this)
        {
            if (Handled) return;

            Handled = true;
        }

        DialogResult = Result.Confirmed;
        OnClose?.Invoke(DialogResult);
        Close();
    }

    /// <summary>
    /// A callback for when the user refuses the action.
    /// Sets <see cref="DialogResult" /> to <see cref="Result.Refused" /> and closes the dialog.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    protected virtual void Refuse_OnClick(object? sender, RoutedEventArgs e)
    {
        lock (this)
        {
            if (Handled) return;

            Handled = true;
        }

        DialogResult = Result.Refused;
        OnClose?.Invoke(DialogResult);
        Close();
    }

    /// <summary>
    /// A callback for when the user cancels the action.
    /// Sets <see cref="DialogResult" /> to <see cref="Result.Cancelled" /> and closes the dialog.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    protected virtual void Cancel_OnClick(object? sender, RoutedEventArgs? e)
    {
        lock (this)
        {
            if (Handled) return;

            Handled = true;
        }

        DialogResult = Result.Cancelled;
        OnClose?.Invoke(DialogResult);
        Close();
    }

    /// <summary>
    /// A callback for when the dialog is closed. Sets <see cref="DialogResult" /> to <see cref="Result.Cancelled" />
    /// if it hasn't already been <see cref="Handled">handled</see>.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    protected virtual void Dialog_OnClosed(object? sender, EventArgs e)
    {
        lock (this)
        {
            if (Handled) return;

            Handled = true;
        }

        DialogResult = Result.Cancelled;
        OnClose?.Invoke(DialogResult);
    }

    // ReSharper restore UnusedParameter.Local

    /// <inheritdoc cref="Window.ShowDialog" />
    /// <returns>A task that can be used to retrieve the result of the dialog when it closes.</returns>
    public new async Task<Result> ShowDialog(Window owner)
    {
        await base.ShowDialog(owner);
        return DialogResult;
    }

    /// <summary>
    /// Shows the dialog as a standalone window.
    /// </summary>
    /// <param name="onClose">An action to be called when the dialog is closed.</param>
    /// <remarks>
    /// This call is non-blocking - it will not wait for the dialog to close and perform the provided action!
    /// </remarks>
    public void ShowStandaloneDialog(Action<Result> onClose)
    {
        OnClose = onClose;
        ShowInTaskbar = true;
        Show();
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
}
