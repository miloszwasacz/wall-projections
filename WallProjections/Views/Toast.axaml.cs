using System.Threading;
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
    /// A mutex to ensure consistent access to <see cref="_openedCount" />.
    /// </summary>
    private readonly Mutex _mutex = new();

    /// <summary>
    /// Defines the <see cref="Text" /> property.
    /// </summary>
    private static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<Toast, string?>(nameof(Text));

    /// <summary>
    /// Defines the <see cref="BoxShadow" /> property.
    /// </summary>
    public static readonly StyledProperty<Avalonia.Media.BoxShadows> BoxShadowProperty =
        AvaloniaProperty.Register<Toast, Avalonia.Media.BoxShadows>(nameof(BoxShadow));

    /// <inheritdoc cref="TextBlock.Text" />
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <inheritdoc cref="Border.BoxShadow" />
    public Avalonia.Media.BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }

    /// <summary>
    /// Opens the toast for the specified duration.
    /// </summary>
    /// <param name="duration">The duration for which the toast should be shown.</param>
    public async void Show(ShowDuration duration)
    {
        _mutex.WaitOne();
        _openedCount++;
        IsVisible = true;
        _mutex.ReleaseMutex();

        await Task.Delay((int)duration);

        _mutex.WaitOne();
        _openedCount--;
        if (_openedCount == 0)
            IsVisible = false;
        _mutex.ReleaseMutex();
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
