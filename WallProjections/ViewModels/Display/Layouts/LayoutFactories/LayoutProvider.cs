using System;
using System.Collections.Generic;
using System.Linq;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Display.Layouts;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.Models.LayoutFactories;

public class LayoutProvider : ILayoutProvider
{
    /// <summary>
    /// Finds all <see cref="ILayoutFactory">Layout Factories</see> that can make layouts automatically.
    /// </summary>
    public LayoutProvider()
    {
        // From: https://stackoverflow.com/questions/67079586/get-all-classes-that-implement-an-interface-and-call-a-function-in-net-core
        _layoutFactories = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => GetType().Namespace.Equals(p.Namespace))
            .Where(p => typeof(ILayoutFactory).IsAssignableFrom(p) && p.IsClass)
            .Select(p => (ILayoutFactory)Activator.CreateInstance(p));

    }

    /// <summary>
    /// Creates <see cref="LayoutProvider"/> with passed in <see cref="ILayoutFactory">Layout Factories</see>
    /// </summary>
    /// <param name="layoutFactories">Collection of <see cref="ILayoutFactory"/></param>
    public LayoutProvider(IEnumerable<ILayoutFactory> layoutFactories)
    {
        _layoutFactories = layoutFactories;
    }

    private readonly IEnumerable<ILayoutFactory> _layoutFactories;

    public ILayout GetLayout(IViewModelProvider vmProvider, Hotspot.Media hotspot)
    {
        foreach (var layoutFactory in _layoutFactories)
        {
            if (layoutFactory.IsCompatibleData(hotspot))
            {
                return layoutFactory.CreateLayout(vmProvider, hotspot);
            }
        }

        return new DescriptionViewModel("Cannot show the information for this hotspot." +
                                        "\nPlease ask a member of staff for help.");
    }
}
