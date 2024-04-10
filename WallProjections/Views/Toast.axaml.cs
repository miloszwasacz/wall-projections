using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

// DO NOT IMPORT `Avalonia.Media` - it breaks `BoxShadowProperty`!

namespace WallProjections.Views;

/// <summary>
/// A simple toast control for showing a message to the user for a short period of time.
/// </summary>
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

    /// <summary>
    /// Defines the <see cref="BoxShadow" /> property.
    /// </summary>
    public static readonly StyledProperty<Avalonia.Media.BoxShadows> BoxShadowProperty =
        AvaloniaProperty.Register<Toast, Avalonia.Media.BoxShadows>(nameof(BoxShadow));

    /// <summary>
    /// Defines the <see cref="Spacing" /> property.
    /// </summary>
    public static readonly StyledProperty<GridLength> SpacingProperty =
        AvaloniaProperty.Register<Toast, GridLength>(nameof(Spacing), new GridLength(0));

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

    /// <inheritdoc cref="Border.BoxShadow" />
    public Avalonia.Media.BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }

    /// <summary>
    /// How much space should be left between the text and the loading spinner.
    /// </summary>
    public GridLength Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>
    /// Opens the toast for the specified duration.
    /// </summary>
    /// <param name="duration">The duration for which the toast should be shown.</param>
    /// <seealso cref="Show()" />
    public async void Show(ShowDuration duration)
    {
        Show();
        await Task.Delay((int)duration);
        Hide();
    }

    /// <summary>
    /// Opens the toast.
    /// </summary>
    /// <seealso cref="Hide" />
    /// <seealso cref="Show(ShowDuration)" />
    public void Show()
    {
        lock (this)
        {
            _openedCount++;
            IsVisible = true;
        }
    }

    /// <summary>
    /// Hides the toast.
    /// </summary>
    /// <seealso cref="Show()" />
    /// <seealso cref="Show(ShowDuration)" />
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
    /// The duration for which the toast should be shown.
    /// </summary>
    public enum ShowDuration
    {
        Short = 2000,
        Long = 3500
    }
}
