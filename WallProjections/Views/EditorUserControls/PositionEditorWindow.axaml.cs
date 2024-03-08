using Avalonia.Controls;
using Avalonia.Input;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Views.EditorUserControls;

public partial class PositionEditorWindow : Window
{
    public PositionEditorWindow()
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
        if (DataContext is not IPositionEditorViewModel vm) return;

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
        if (DataContext is not IPositionEditorViewModel vm) return;

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
        if (DataContext is not IPositionEditorViewModel vm) return;

        vm.UpdateSelectedHotspot();
    }

    // ReSharper restore UnusedParameter.Local
}
