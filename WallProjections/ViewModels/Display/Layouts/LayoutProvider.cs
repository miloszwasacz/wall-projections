using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
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
    internal const string ErrorDescription = "Cannot show the information for this hotspot.\n" +
                                             "Please ask a member of staff for help.";

    /// <summary>
    /// All available <see cref="LayoutFactory">Layout Factories</see>
    /// </summary>
    private readonly IEnumerable<LayoutFactory> _layoutFactories;

    // ReSharper disable once UnusedParameter.Local
    /// <summary>
    /// Finds all <see cref="LayoutFactory">Layout Factories</see> that can make layouts automatically.
    /// </summary>
    /// <param name="loggerFactory">A factory for creating loggers.</param>
    public LayoutProvider(ILoggerFactory loggerFactory)
    {
#if RELEASE
        var logger = loggerFactory.CreateLogger<LayoutProvider>();
#endif
        // From: https://stackoverflow.com/questions/67079586/get-all-classes-that-implement-an-interface-and-call-a-function-in-net-core
        _layoutFactories = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => GetType().Namespace?.Equals(p.Namespace) ?? false)
            .Where(p => typeof(LayoutFactory).IsAssignableFrom(p) && p.IsClass)
#if !RELEASE
            // In debug, we want to throw an exception if a layout factory is not correctly implemented
            .Select(p => (LayoutFactory)Activator.CreateInstance(p)!);
#else
            // In release, we want to ignore any layout factories that are not correctly implemented
            .Select(p =>
            {
                try
                {
                    return Activator.CreateInstance(p) as LayoutFactory
                           ?? throw new InvalidCastException();
                }
                catch (Exception e)
                {
                    logger.LogWarning(
                        e, "LayoutFactory of type {LayoutFactoryType} is not correctly implemented", p.Name
                    );
                    return null;
                }
            })
            .Where(p => p is not null)
            .Select(p => p!);
#endif
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
    public Layout GetWelcomeLayout() => new WelcomeViewModel();

    /// <inheritdoc />
    public Layout GetMessageLayout(string title, string description) => new MessageViewModel(title, description);

    /// <inheritdoc />
    public Layout GetErrorLayout(string message, string title = ILayoutProvider.DefaultErrorTitle) =>
        new ErrorViewModel(title, message);
}
