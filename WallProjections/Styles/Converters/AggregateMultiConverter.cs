using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace WallProjections.Styles.Converters;

/// <summary>
/// A multi-value converter that aggregates values of type <typeparamref name="T"/> into a single value of type <typeparamref name="TRes"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TAcc"></typeparam>
/// <typeparam name="TRes"></typeparam>
public abstract class AggregateMultiConverter<T, TAcc, TRes> : IMultiValueConverter
{
    private readonly IValueConverter _converter;
    private readonly TAcc _zero;
    private readonly Func<TAcc, T, TAcc> _aggregate;
    private readonly Func<TAcc, object?, TRes> _resultTransform;

    /// <summary>
    /// Creates a new instance of <see cref="AggregateMultiConverter{T, TAcc, TRes}"/>.
    /// </summary>
    /// <param name="converter">The converter to use to convert individual values.</param>
    /// <param name="aggregate">The function to use to aggregate values.</param>
    /// <param name="resultTransform">The function applied to the result of the aggregation.</param>
    /// <param name="zero">The initial value of the accumulator.</param>
    protected AggregateMultiConverter(
        IValueConverter converter,
        Func<TAcc, T, TAcc> aggregate,
        Func<TAcc, object?, TRes> resultTransform,
        TAcc zero = default!
    )
    {
        _converter = converter;
        _zero = zero;
        _aggregate = aggregate;
        _resultTransform = resultTransform;
    }


    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            return values
                .Select(v => (T)_converter.Convert(v, typeof(T), null, culture)!)
                .Aggregate(_zero, _aggregate, result => _resultTransform(result, parameter));
        }
        catch (Exception e)
        {
            return new BindingNotification(e, BindingErrorType.Error);
        }
    }
}
