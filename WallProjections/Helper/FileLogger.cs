using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace WallProjections.Helper;

/// <summary>
/// A logger that writes log messages to a text file.
/// </summary>
public class FileLogger : ILogger
{
    /// <summary>
    /// The <a href="https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line#log-category">category</a>
    /// for which the logger was created.
    /// </summary>
    private readonly string _categoryName;

    /// <summary>
    /// The <see cref="StreamWriter" /> used to write log messages to a text file.
    /// </summary>
    private readonly StreamWriter _logFileWriter;

    /// <summary>
    /// Creates a new <see cref="FileLogger" /> that writes log messages to the specified file.
    /// </summary>
    /// <param name="categoryName">
    /// The <a href="https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line#log-category">category</a>
    /// for which the logger was created.
    /// </param>
    /// <param name="logFileWriter">The <see cref="StreamWriter" /> used to write log messages to a text file.</param>
    public FileLogger(string categoryName, StreamWriter logFileWriter)
    {
        _categoryName = categoryName;
        _logFileWriter = logFileWriter;
    }

    /// <inheritdoc />
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        // Ensure that only information level and higher logs are recorded
        if (!IsEnabled(logLevel)) return;

        // Get the formatted log message
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var message = formatter(state, exception);

        //Write log messages to text file
        _logFileWriter.WriteLine($"[{timestamp}] [{logLevel}] [{_categoryName}] {message}");
        _logFileWriter.Flush();
    }

    /// <inheritdoc />
    /// <remarks>Is only enabled for log levels of <see cref="LogLevel.Information"/> and higher.</remarks>
    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    /// <inheritdoc />
    /// <returns>Always <i>null</i>.</returns>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
}

/// <summary>
/// A provider for <see cref="FileLogger" /> instances.
/// </summary>
public class FileLoggerProvider : ILoggerProvider
{
    /// <summary>
    /// The path to the default folder where log files are stored.
    /// </summary>
    public static readonly string DefaultLogFolderPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WallProjections",
        "Logs"
    );

    /// <summary>
    /// The <see cref="StreamWriter" /> used to write log messages to a text file.
    /// </summary>
    private readonly StreamWriter _logFileWriter;

    /// <summary>
    /// Creates a new <see cref="FileLoggerProvider" /> that writes log messages to the specified file.
    /// </summary>
    /// <param name="logFilePath">The path to the file to which log messages should be written.</param>
    /// <exception cref="Exception">
    /// Throws the same exceptions as <see cref="Directory.CreateDirectory" /> <see cref="StreamWriter" />.
    /// </exception>
    public FileLoggerProvider(string logFilePath)
    {
        var logDirectory = Path.GetDirectoryName(logFilePath);
        if (logDirectory is not null && !Directory.Exists(logDirectory))
            Directory.CreateDirectory(logDirectory);

        _logFileWriter = new StreamWriter(logFilePath, append: true);
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName) => new FileLogger(categoryName, _logFileWriter);

    /// <inheritdoc />
    public void Dispose()
    {
        _logFileWriter.Dispose();
        GC.SuppressFinalize(this);
    }
}
