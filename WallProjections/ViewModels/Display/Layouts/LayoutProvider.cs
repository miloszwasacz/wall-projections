using System;
using System.Collections.Generic;
using System.Linq;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.ViewModels.Display.Layouts;

/// <inheritdoc cref="ILayoutProvider" />
public class LayoutProvider : ILayoutProvider
{
    /// <summary>
    /// Description for when no layout could be found for a hotspot
    /// </summary>
    private const string ErrorDescription = "Cannot show the information for this hotspot.\n" +
                                            "Please ask a member of staff for help.";

    /// <summary>
    /// All available <see cref="LayoutFactory">Layout Factories</see>
    /// </summary>
    private readonly IEnumerable<LayoutFactory> _layoutFactories;

    /// <summary>
    /// Finds all <see cref="LayoutFactory">Layout Factories</see> that can make layouts automatically.
    /// </summary>
    public LayoutProvider()
    {
        // From: https://stackoverflow.com/questions/67079586/get-all-classes-that-implement-an-interface-and-call-a-function-in-net-core
        _layoutFactories = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => GetType().Namespace?.Equals(p.Namespace) ?? false)
            .Where(p => typeof(LayoutFactory).IsAssignableFrom(p) && p.IsClass)
            .Select(p => (LayoutFactory)Activator.CreateInstance(p)!);
    }

    /// <summary>
    /// Creates <see cref="LayoutProvider"/> with passed in <see cref="LayoutFactory">Layout Factories</see>
    /// </summary>
    /// <param name="layoutFactories">Collection of <see cref="LayoutFactory"/></param>
    public LayoutProvider(IEnumerable<LayoutFactory> layoutFactories)
    {
        _layoutFactories = layoutFactories;
    }

    /// <inheritdoc />
    public Layout GetLayout(IViewModelProvider vmProvider, Hotspot.Media hotspot)
    {
        foreach (var layoutFactory in _layoutFactories)
        {
            if (layoutFactory.IsCompatibleData(hotspot))
                return layoutFactory.CreateLayout(vmProvider, hotspot);
        }

        return ((ILayoutProvider)this).GetErrorLayout(ErrorDescription);
    }

    /// <inheritdoc />
    public Layout GetSimpleDescriptionLayout(string title, string description) =>
        new DescriptionViewModel(title, description);
}
