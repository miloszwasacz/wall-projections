using System;
using System.Globalization;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;

namespace WallProjections.Views;

/// <summary>
/// A circle representing a hotspot. Set the <see cref="StyledElement.DataContext" />
/// </summary>
public partial class HotspotCircle : Panel, IDisposable
{
    /// <summary>
    /// The time required to pulse the hotspot (one direction only)
    /// </summary>
    public static readonly TimeSpan PulseTime = TimeSpan.FromSeconds(1);

    #region Styled/Direct properties

    /// <summary>
    /// A <see cref="StyledProperty{T}">StyledProperty</see> that defines the <see cref="Diameter" /> property.
    /// </summary>
    public static readonly StyledProperty<double> DiameterProperty =
        AvaloniaProperty.Register<HotspotCircle, double>(nameof(Diameter));

    public static readonly DirectProperty<HotspotCircle, HotspotState> HotspotStateProperty =
        AvaloniaProperty.RegisterDirect<HotspotCircle, HotspotState>(
            nameof(HotspotState),
            o => o.HotspotState,
            (o, v) => o.HotspotState = v
        );

    /// <summary>
    /// A <see cref="DirectProperty{T, TP}">DirectProperty</see> that defines the <see cref="Pulse" /> property.
    /// </summary>
    public static readonly DirectProperty<HotspotCircle, bool> PulseProperty =
        AvaloniaProperty.RegisterDirect<HotspotCircle, bool>(
            nameof(Pulse),
            o => o.Pulse,
            (o, v) => o.Pulse = v
        );

    #endregion

    #region Backing fields

    /// <summary>
    /// The subject that holds the value of <see cref="HotspotState" />
    /// </summary>
    private readonly BehaviorSubject<HotspotState> _hotspotState = new(HotspotState.None);

    /// <summary>
    /// The subject that holds the value of <see cref="Pulse" />
    /// </summary>
    private readonly BehaviorSubject<bool> _pulse = new(false);

    #endregion

    /// <summary>
    /// The diameter of the hotspot
    /// </summary>
    public double Diameter
    {
        get => GetValue(DiameterProperty);
        set => SetValue(DiameterProperty, value);
    }

    public HotspotState HotspotState
    {
        get => _hotspotState.Value;
        set
        {
            var oldValue = _hotspotState.Value;
            _hotspotState.OnNext(value);
            lock (_pulse)
            {
                RaisePropertyChanged(HotspotStateProperty, oldValue, value);
                if (value == HotspotState.Active)
                {
                    StartPulsing();
                }
                else
                {
                    Pulse = false;
                }
            }
            
        }
    }

    /// <summary>
    /// Whether the hotspot should be scaled up
    /// </summary>
    private bool Pulse
    {
        get => _pulse.Value && HotspotState == HotspotState.Active;
        set
        {
            var oldValue = _pulse.Value;
            _pulse.OnNext(value);
            RaisePropertyChanged(PulseProperty, oldValue, value);
        }
    }

    public HotspotCircle()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Starts pulsing the hotspot
    /// </summary>
    /// <remarks>Stops pulsing if the <see cref="HotspotState"/> is no longer set to active</remarks>
    private async void StartPulsing()
    {
        while (true)
        {
            lock (_pulse)
            {
                Pulse = false;

                if (HotspotState != HotspotState.Active)
                    return;
            }

            await Task.Delay(PulseTime);

            lock (_pulse)
            {
                if (HotspotState != HotspotState.Active)
                    return;

                Pulse = true;
            }

            await Task.Delay(PulseTime);
        }
    }

    public void Dispose()
    {
        _hotspotState.Dispose();
        GC.SuppressFinalize(this);
    }
}

public class FullArcDiameterConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double diameter) return 0.0;

        return diameter * 1.25 + 20;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
