using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.Logging;
using WallProjections.Helper;

[assembly: InternalsVisibleTo("WallProjections.Test")]

namespace WallProjections;

// ReSharper disable once ClassNeverInstantiated.Global
[ExcludeFromCodeCoverage(Justification = "This is the main entry point, which uses not testable application lifetime")]
internal class Program
{
    /// <summary>
    /// Initialization code. Don't use any Avalonia, third-party APIs or any
    /// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    /// yet and stuff might break.
    /// </summary>
    /// <param name="args">Application arguments</param>
    [STAThread]
    public static void Main(string[] args) => WithUnhandledExceptionLogging(() =>
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            var logPath = Path.Combine(
                FileLoggerProvider.DefaultLogFolderPath,
                $"WallProjections_{DateTime.Now:yyyy-MM-dd}.log"
            );

            if (args.Contains("--trace"))
                builder.AddFilter(level => level >= LogLevel.Trace);

            builder.AddSimpleConsole(options => options.TimestampFormat = "HH:mm:ss ");
            builder.AddProvider(new FileLoggerProvider(logPath));
        });
        var logger = loggerFactory.CreateLogger(nameof(Program));
        logger.LogInformation("Starting application");

        try
        {
            BuildAvaloniaApp(loggerFactory).StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Unhandled exception occurred");
        }

        logger.LogInformation("Closing application");
    });

    /// <summary>
    /// Avalonia configuration, don't remove.
    /// </summary>
    private static AppBuilder BuildAvaloniaApp(ILoggerFactory loggerFactory) =>
        AppBuilder.Configure(() => new App(loggerFactory))
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();

    /// <summary>
    /// Run the program with logging of unhandled exceptions to <b>stderr</b>.
    /// </summary>
    /// <param name="program">The program to run.</param>
    private static void WithUnhandledExceptionLogging(Action program)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options => options.TimestampFormat = "HH:mm:ss ");
        });
        var logger = loggerFactory.CreateLogger("Unhandled");

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var exception = (Exception)e.ExceptionObject;
            logger.LogCritical(exception, "Unhandled exception occurred");
        };

        program();
    }

    // ReSharper disable once UnusedMember.Local
    /// <summary>
    /// Don't use this method. It is only used by the visual designer.
    /// Use <see cref="BuildAvaloniaApp(ILoggerFactory)"/> instead.
    /// </summary>
    [Obsolete("This method is only used by the visual designer.", true)]
    private static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
}
