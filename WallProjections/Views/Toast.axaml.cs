using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;

namespace WallProjections.Views;

/// <summary>
/// A simple toast control for showing a message to the user for a short period of time.
/// </summary>
[PseudoClasses(":information", ":success", ":warning", ":error")]
public class Toast : TemplatedControl
{
    /// <summary>
    /// The number of times the toast has been opened (used for keeping track of when to hide the toast).
    /// </summary>
    private int _openedCount;

    /// <summary>
    /// The backing field for the <see cref="IsLoading" /> property.
    /// </summary>
    private bool _isLoading;

    /// <summary>
    /// Defines the <see cref="Text" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<Toast, string?>(nameof(Text));

    /// <summary>
    /// Defines the <see cref="IsLoading" /> property.
    /// </summary>
    public static readonly DirectProperty<Toast, bool> IsLoadingProperty =
        AvaloniaProperty.RegisterDirect<Toast, bool>(
            nameof(IsLoading),
            o => o.IsLoading,
            (o, v) => o.IsLoading = v
        );

    /// <inheritdoc cref="TextBlock.Text" />
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Whether the toast should show a loading spinner.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set => SetAndRaise(IsLoadingProperty, ref _isLoading, value);
    }

    /// <summary>
    /// Opens the toast for the specified duration.
    /// </summary>
    /// <param name="duration">The duration for which the toast should be shown.</param>
    /// <param name="type">The type of the notification.</param>
    /// <seealso cref="Show(NotificationType)" />
    public async void Show(ShowDuration duration, NotificationType type = NotificationType.Information)
    {
        Show(type);
        await Task.Delay((int)duration);
        Hide();
    }

    /// <summary>
    /// Opens the toast.
    /// </summary>
    /// <param name="type">The type of the notification.</param>
    /// <seealso cref="Hide" />
    /// <seealso cref="Show(ShowDuration, NotificationType)" />
    public void Show(NotificationType type = NotificationType.Information)
    {
        lock (this)
        {
            SetType(type);
            _openedCount++;
            IsVisible = true;
        }
    }

    /// <summary>
    /// Hides the toast.
    /// </summary>
    /// <seealso cref="Show(NotificationType)" />
    /// <seealso cref="Show(ShowDuration, NotificationType)" />
    public void Hide()
    {
        lock (this)
        {
            _openedCount = Math.Max(0, _openedCount - 1);
            if (_openedCount == 0)
                IsVisible = false;
        }
    }

    /// <summary>
    /// Adds appropriate pseudo-classes for the specified notification type.
    /// </summary>
    /// <param name="type">The type of the notification.</param>
    private void SetType(NotificationType type)
    {
        PseudoClasses.Set(":information", type == NotificationType.Information);
        PseudoClasses.Set(":success", type == NotificationType.Success);
        PseudoClasses.Set(":warning", type == NotificationType.Warning);
        PseudoClasses.Set(":error", type == NotificationType.Error);
    }

    /// <summary>
    /// The duration for which the toast should be shown.
    /// </summary>
    public enum ShowDuration
    {
        Short = 2000,
        Long = 3500
    }
}
