using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace WallProjections.Views.EditorUserControls;

public class DescriptionEditor : TemplatedControl
{
    /// <summary>
    /// A <see cref="StyledProperty{GridLength}">StyledProperty</see> that defines the <see cref="Spacing" /> property.
    /// </summary>
    public static readonly StyledProperty<GridLength> SpacingProperty =
        AvaloniaProperty.Register<DescriptionEditor, GridLength>(nameof(Spacing));

    /// <summary>
    /// A routed event that is raised when the import button is clicked.
    /// </summary>
    private static readonly RoutedEvent<RoutedEventArgs> ImportDescriptionEvent =
        RoutedEvent.Register<DescriptionEditor, RoutedEventArgs>(nameof(ImportDescription), RoutingStrategies.Bubble);

    /// <summary>
    /// Spacing between the elements in the editor.
    /// </summary>
    public GridLength Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>
    /// An event that is raised when the import button is clicked.
    /// </summary>
    public event EventHandler<RoutedEventArgs> ImportDescription
    {
        add => AddHandler(ImportDescriptionEvent, value);
        remove => RemoveHandler(ImportDescriptionEvent, value);
    }

    /// <summary>
    /// Raises the <see cref="ImportDescription" /> event.
    /// </summary>
    /// <param name="sender">The clicked button.</param>
    public void ImportButton_OnClick(object? sender)
    {
        RaiseEvent(new RoutedEventArgs(ImportDescriptionEvent, sender));
    }
}
