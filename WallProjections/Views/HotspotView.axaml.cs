using System;
using Avalonia.Controls;

namespace WallProjections.Views;

public partial class HotspotView : ItemsControl
{
    protected override Type StyleKeyOverride => typeof(ItemsControl);

    public HotspotView()
    {
        InitializeComponent();
    }
}
