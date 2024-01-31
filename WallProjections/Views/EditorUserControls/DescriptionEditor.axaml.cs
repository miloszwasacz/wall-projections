using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace WallProjections.Views.EditorUserControls;

public class DescriptionEditor : TemplatedControl
{
    public static readonly StyledProperty<GridLength> SpacingProperty =
        AvaloniaProperty.Register<DescriptionEditor, GridLength>(nameof(Spacing));

    public GridLength Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }
}
