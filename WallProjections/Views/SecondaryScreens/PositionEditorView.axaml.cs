using Avalonia.Controls;
using Avalonia.Input;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;

namespace WallProjections.Views.SecondaryScreens;

// ReSharper disable once UnusedType.Global
public partial class PositionEditorView : UserControl
{
    public PositionEditorView()
    {
        InitializeComponent();
    }

    // ReSharper disable UnusedParameter.Local

    /// <summary>
    /// Updates the displayed position of the hotspot when the mouse is moved.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments holding the new position of the mouse.</param>
    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (DataContext is not AbsPositionEditorViewModel vm) return;

        var position = e.GetPosition(this);
        vm.SetPosition(position.X, position.Y);
    }

    /// <summary>
    /// Updates the radius of the hotspot when the mouse wheel is scrolled.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments holding delta of mouse wheel change.</param>
    private void OnScroll(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is not AbsPositionEditorViewModel vm) return;

        // e.Delta.Y > 0 -> UP (make larger)
        // e.Delta.Y < 0 -> DOWN (make smaller)
        vm.ChangeRadius(e.Delta.Y);
    }

    /// <summary>
    /// Saves the current position and radius of the hotspot when the mouse is pressed.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused, data from the VM is used instead).</param>
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not AbsPositionEditorViewModel vm) return;

        vm.UpdateSelectedHotspot();
    }

    /// <summary>
    /// Handles key presses:
    /// <ul>
    ///     <li>
    ///         <b>Escape</b>: Exit <see cref="AbsPositionEditorViewModel.IsInEditMode">edit mode</see>
    ///                        for <see cref="PositionEditorView.DataContext" />
    ///     </li>
    /// </ul>
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments containing the key that was pressed.</param>
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not AbsPositionEditorViewModel vm) return;

        if (e.Key == Key.Escape)
            vm.IsInEditMode = false;
    }

    // ReSharper restore UnusedParameter.Local
}
