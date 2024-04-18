using System;
using System.Globalization;
using Avalonia.Data.Converters;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;

namespace WallProjections.Views.Converters;


public class StateConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Console.WriteLine("value: " + value + ", parameter: " + parameter);
        if (value is HotspotState state && parameter is HotspotState param) return state == param;
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

