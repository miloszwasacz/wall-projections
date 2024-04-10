using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.Logging;
using WallProjections.Helper;
using WallProjections.Helper.Interfaces;

[assembly: InternalsVisibleTo("WallProjections.Test")]

namespace WallProjections;

// ReSharper disable once ClassNeverInstantiated.Global
internal class Program
{
    /// <summary>
    /// Initialization code. Don't use any Avalonia, third-party APIs or any
    /// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    /// yet and stuff might break.
    /// </summary>
    /// <param name="args">Application arguments</param>
    [STAThread]
    [ExcludeFromCodeCoverage]
    public static void Main(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var pythonProxy = new PythonProxy(loggerFactory);
        var pythonHandler = new PythonHandler(pythonProxy, loggerFactory);
        BuildAvaloniaApp(pythonHandler, loggerFactory).StartWithClassicDesktopLifetime(args);
        pythonHandler.Dispose();
        pythonProxy.Dispose();
    }

    /// <summary>
    /// Avalonia configuration, don't remove; also used by visual designer.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private static AppBuilder BuildAvaloniaApp(IPythonHandler pythonHandler, ILoggerFactory loggerFactory)
    {
        return AppBuilder.Configure(() => new App(pythonHandler, loggerFactory))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
    }
}
