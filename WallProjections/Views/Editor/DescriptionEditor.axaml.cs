using Avalonia;
using Avalonia.Controls;

namespace WallProjections.Views.Editor;

public partial class DescriptionEditor : UserControl
{
    /// <summary>
    /// The backing field for the <see cref="MenuItems" /> property.
    /// </summary>
    private Controls _menuItems = new();

    /// <summary>
    /// A <see cref="DirectProperty{TOwner,TValue}">DirectProperty</see> that defines the <see cref="MenuItems" /> property.
    /// </summary>
    public static readonly DirectProperty<DescriptionEditor, Controls> MenuItemsProperty =
        AvaloniaProperty.RegisterDirect<DescriptionEditor, Controls>(
            nameof(MenuItems),
            o => o.MenuItems,
            (o, v) => o.MenuItems = v
        );

    /// <summary>
    /// The controls displayed to the right of Import button.
    /// </summary>
    public Controls MenuItems
    {
        get => _menuItems;
        set => SetAndRaise(MenuItemsProperty, ref _menuItems, value);
    }

    public DescriptionEditor()
    {
        InitializeComponent();
    }
}
