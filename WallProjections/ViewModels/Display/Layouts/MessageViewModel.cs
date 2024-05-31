using System;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.ViewModels.Display.Layouts;

/// <summary>
/// A viewmodel for a view that displays a message with a title and description.
/// </summary>
public class MessageViewModel : DescriptionViewModel
{
    /// <summary>
    /// Creates a new <see cref="MessageViewModel" /> with the given <paramref name="title" /> and <paramref name="description" />.
    /// </summary>
    /// <param name="title">The message title.</param>
    /// <param name="description">The full description.</param>
    /// <param name="deactivateAfter">
    /// The time after which the layout should deactivate.
    /// If <i>null</i>, the layout will deactivate after the <see cref="Layout.DefaultDeactivationTime">default time</see>.
    /// </param>
    public MessageViewModel(
        string title,
        string description,
        TimeSpan? deactivateAfter = null
    ) : base(title, description, deactivateAfter ?? DefaultDeactivationTime)
    {
    }
}
