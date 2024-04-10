using System;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;

namespace WallProjections.Views;

public class LoadingRing : ProgressRing
{
    protected override Type StyleKeyOverride => typeof(ProgressRing);

    public LoadingRing()
    {
        IsIndeterminate = true;
        Background = Brushes.Transparent;
        MinWidth = 0;
        MinHeight = 0;
    }
}
