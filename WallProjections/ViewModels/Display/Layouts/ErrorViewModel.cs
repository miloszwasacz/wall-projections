using System;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.ViewModels.Display.Layouts;

/// <summary>
/// A viewmodel for a view that displays an error with a title and description.
/// </summary>
public class ErrorViewModel : DescriptionViewModel
{
    /// <summary>
    /// Creates a new <see cref="ErrorViewModel" /> with the given <paramref name="title" /> and <paramref name="message" />.
    /// </summary>
    /// <param name="title">The title of the error.</param>
    /// <param name="message">The message of the error.</param>
    /// <param name="deactivateAfter">
    /// The time after which the layout should deactivate.
    /// If <i>null</i>, the layout will deactivate after the <see cref="Layout.DefaultDeactivationTime">default time</see>.
    /// </param>
    public ErrorViewModel(
        string title,
        string message,
        TimeSpan? deactivateAfter = null
    ) : base(title, message, deactivateAfter ?? DefaultDeactivationTime)
    {
    }
}
