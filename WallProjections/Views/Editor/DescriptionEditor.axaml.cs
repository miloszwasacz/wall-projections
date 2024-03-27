using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace WallProjections.Views.Editor
{
    public class DescriptionEditor : TemplatedControl
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
        /// A <see cref="StyledProperty{GridLength}">StyledProperty</see> that defines the <see cref="Spacing" /> property.
        /// </summary>
        public static readonly StyledProperty<GridLength> SpacingProperty =
            AvaloniaProperty.Register<DescriptionEditor, GridLength>(nameof(Spacing));

        /// <summary>
        /// A <see cref="StyledProperty{GridLength}">StyledProperty</see> that defines the <see cref="MenuItemSpacing" /> property.
        /// </summary>
        public static readonly StyledProperty<GridLength> MenuItemSpacingProperty =
            AvaloniaProperty.Register<DescriptionEditor, GridLength>(nameof(MenuItemSpacing));

        /// <summary>
        /// A routed event that is raised when the import button is clicked.
        /// </summary>
        private static readonly RoutedEvent<RoutedEventArgs> ImportDescriptionEvent =
            RoutedEvent.Register<DescriptionEditor, RoutedEventArgs>(
                nameof(ImportDescription),
                RoutingStrategies.Bubble
            );

        /// <summary>
        /// The controls displayed to the right of Import button.
        /// </summary>
        [Content]
        public Controls MenuItems
        {
            get => _menuItems;
            set => SetAndRaise(MenuItemsProperty, ref _menuItems, value);
        }

        /// <summary>
        /// Spacing between the elements in the editor.
        /// </summary>
        public GridLength Spacing
        {
            get => GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        /// <summary>
        /// Spacing between the <see cref="MenuItems" />.
        /// </summary>
        public GridLength MenuItemSpacing
        {
            get => GetValue(MenuItemSpacingProperty);
            set => SetValue(MenuItemSpacingProperty, value);
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
}

namespace WallProjections.Views.Converters
{
    /// <summary>
    /// Converts a <see cref="GridLength" /> to a <see cref="double" />.
    /// </summary>
    public class SpacingConverter : IValueConverter
    {
        // ReSharper disable once UnusedMember.Global
        public static readonly SpacingConverter Instance = new();

        /// <inheritdoc />
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is GridLength gridLength && targetType == typeof(double))
                return gridLength.Value;

            return 0;
        }

        /// <inheritdoc />
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double d && targetType == typeof(GridLength))
                return new GridLength(d);

            return GridLength.Auto;
        }
    }
}
