using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Views.EditorUserControls;

public partial class MediaEditor : UserControl
{
    /// <summary>
    /// A routed event that is raised when the user wants to add media.
    /// </summary>
    /// <seealso cref="AddMedia" />
    private static readonly RoutedEvent<RoutedEventArgs> AddMediaEvent =
        RoutedEvent.Register<MediaEditor, RoutedEventArgs>(nameof(AddMedia), RoutingStrategies.Bubble);

    /// <summary>
    /// A routed event that is raised when opening the explorer fails.
    /// </summary>
    /// <seealso cref="OpenExplorerFailed" />
    private static readonly RoutedEvent<RoutedEventArgs> OpenExplorerFailedEvent =
        RoutedEvent.Register<MediaEditor, RoutedEventArgs>(nameof(OpenExplorerFailed), RoutingStrategies.Bubble);

    /// <summary>
    /// An event that is raised when the user wants to add media.
    /// </summary>
    public event EventHandler<RoutedEventArgs> AddMedia
    {
        add => AddHandler(AddMediaEvent, value);
        remove => RemoveHandler(AddMediaEvent, value);
    }

    /// <summary>
    /// An event that is raised when opening the explorer fails.
    /// </summary>
    public event EventHandler<RoutedEventArgs> OpenExplorerFailed
    {
        add => AddHandler(OpenExplorerFailedEvent, value);
        remove => RemoveHandler(OpenExplorerFailedEvent, value);
    }

    public MediaEditor()
    {
        InitializeComponent();
    }

    // ReSharper disable UnusedParameter.Local

    /// <summary>
    /// A callback for when the open explorer button is clicked.
    /// <br /><br />
    /// Checks if the sender's <see cref="StyledElement.DataContext" /> is a <see cref="IThumbnailViewModel" />,
    /// and if so, calls <see cref="IThumbnailViewModel.OpenInExplorer" />. Then, if the operation failed,
    /// raises an <see cref="OpenExplorerFailedEvent" />.
    /// </summary>
    /// <param name="sender">The sender of the <see cref="Button.Click" /> event.</param>
    /// <param name="e">The <see cref="Button.Click" /> event arguments (unused).</param>
    private void ItemOpenExplorer_OnClick(object? sender, RoutedEventArgs e)
    {
        var control = sender as ContentControl;
        var vm = control?.DataContext as IThumbnailViewModel;
        var result = vm?.OpenInExplorer() ?? false;
        if (!result)
            RaiseEvent(new RoutedEventArgs(OpenExplorerFailedEvent, this));
    }

    /// <summary>
    /// A callback for when the add media button is clicked.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private void AddMedia_OnClick(object? sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(AddMediaEvent, this));
    }

    // ReSharper restore UnusedParameter.Local
}
