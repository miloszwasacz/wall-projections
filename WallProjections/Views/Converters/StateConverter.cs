using System;
using System.Globalization;
using Avalonia.Data.Converters;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;

namespace WallProjections.Views.Converters;

/// <summary>
/// A converter which converts <see cref="HotspotState"/> into a boolean 
/// </summary>
public class StateConverter : IValueConverter
{
    /// <summary>
    /// Compares the value to the parameter and returns true if they are the same and
    /// false otherwise
    /// </summary>
    /// <param name="value">The <see cref="HotspotState"/> to be converted</param>
    /// <param name="targetType"></param>
    /// <param name="parameter">The <see cref="HotspotState"/> to be compared to</param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is HotspotState state && parameter is HotspotState param) return state == param;
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

