using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using WallProjections.ViewModels.Editor;

namespace WallProjections.Views.Editor;

public partial class HotspotList : UserControl
{
    /// <summary>
    /// A routed event that is raised when the user creates a new hotspot.
    /// </summary>
    private static readonly RoutedEvent NewHotspotEvent =
        RoutedEvent.Register<HotspotList, RoutedEventArgs>(nameof(AddHotspot), RoutingStrategies.Bubble);

    /// <summary>
    /// A routed event that is raised when the user deletes a hotspot.
    /// </summary>
    private static readonly RoutedEvent<DeleteArgs> DeleteHotspotEvent =
        RoutedEvent.Register<HotspotList, DeleteArgs>(nameof(DeleteHotspot), RoutingStrategies.Bubble);

    /// <summary>
    /// An event that is raised when the user creates a new hotspot.
    /// </summary>
    public event EventHandler AddHotspot
    {
        add => AddHandler(NewHotspotEvent, value);
        remove => RemoveHandler(NewHotspotEvent, value);
    }

    /// <summary>
    /// An event that is raised when the user deletes a hotspot.
    /// </summary>
    public event EventHandler<DeleteArgs> DeleteHotspot
    {
        add => AddHandler(DeleteHotspotEvent, value);
        remove => RemoveHandler(DeleteHotspotEvent, value);
    }

    public HotspotList()
    {
        InitializeComponent();
    }

    // ReSharper disable UnusedParameter.Local

    /// <summary>
    /// A callback for when the user adds a new hotspot.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private void Add_OnClick(object? sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(NewHotspotEvent, this));
    }

    /// <summary>
    /// A callback for when the user deletes a hotspot.
    /// The hotspot is fetched from <paramref name="sender" />'s <see cref="Control.Tag" /> property.
    /// </summary>
    /// <param name="sender">
    /// The sender of the event.
    /// Must have a <see cref="Control.Tag" /> property of type <see cref="EditorHotspotViewModel" />.
    /// </param>
    /// <param name="e">The event arguments (unused).</param>
    private void Delete_OnClick(object? sender, RoutedEventArgs e)
    {
        var control = sender as Control;
        if (control?.Tag is not EditorHotspotViewModel hotspot) return;

        RaiseEvent(new DeleteArgs(this, hotspot));
    }

    //ReSharper restore UnusedParameter.Local

    /// <summary>
    /// Arguments for the <see cref="DeleteHotspot" /> event.
    /// </summary>
    public class DeleteArgs : RoutedEventArgs
    {
        /// <summary>
        /// The hotspot to be deleted.
        /// </summary>
        public EditorHotspotViewModel Hotspot { get; }

        /// <inheritdoc cref="DeleteArgs" />
        /// <seealso cref="HotspotList.Delete_OnClick" />
        public DeleteArgs(
            object? source,
            EditorHotspotViewModel hotspot
        ) : base(DeleteHotspotEvent, source)
        {
            Hotspot = hotspot;
        }
    }
}
