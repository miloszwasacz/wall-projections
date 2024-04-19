using System;
using Avalonia.Markup.Xaml;

namespace WallProjections.Styles.WinUI3;

// ReSharper disable once InconsistentNaming
public class WinUI3 : Avalonia.Styling.Styles
{
    public WinUI3(IServiceProvider? sp = null)
    {
        AvaloniaXamlLoader.Load(sp, this);
    }
}
