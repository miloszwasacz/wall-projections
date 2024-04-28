using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using WallProjections.Models.Interfaces;

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
    /// The maximum number of log files to keep.
    /// </summary>
    private const int MaxLogFiles = 100;

    /// <summary>
    /// The path to the default folder where log files are stored.
    /// </summary>
    public static readonly string DefaultLogFolderPath = Path.Combine(IFileHandler.AppDataFolderPath, "Logs");

    /// <summary>
    /// The default path format for log files.
    /// </summary>
    public static readonly string DefaultLogFilePath = Path.Combine(DefaultLogFolderPath, "WallProjections");

    /// <summary>
    /// Creates the path to a log file with the specified extension and the current date.
    /// </summary>
    /// <param name="basePath">The base path of the log file, including the directory.</param>
    /// <param name="extension">The extension of the log file.</param>
    private static string GetLogPath(string basePath, string extension = "log") => Path.Combine(
        basePath,
        $"_{DateTime.Now:yyyy-MM-dd}.{extension}"
    );

    /// <summary>
    /// The <see cref="StreamWriter" /> used to write log messages to a text file.
    /// </summary>
    private readonly StreamWriter _logFileWriter;

    /// <summary>
    /// Creates a new <see cref="FileLoggerProvider" /> that writes log messages to the specified file.
    /// </summary>
    /// <param name="logFilePath">
    /// The base of the path to the file to which log messages should be written
    /// (the current date will be appended to the file name).
    /// </param>
    /// <exception cref="Exception">
    /// Throws the same exceptions as <see cref="Directory.CreateDirectory" /> and <see cref="StreamWriter" />.
    /// </exception>
    public FileLoggerProvider(string logFilePath)
    {
        var logDirectory = Path.GetDirectoryName(logFilePath);
        if (logDirectory is not null && !Directory.Exists(logDirectory))
            Directory.CreateDirectory(logDirectory);

        var baseLogFileName = Path.GetFileNameWithoutExtension(logFilePath);
        var logFileExtension = Path.GetExtension(logFilePath);
        var basePathWithDir = logDirectory is not null
            ? Path.Combine(logDirectory, baseLogFileName)
            : baseLogFileName;
        var logPath = GetLogPath(basePathWithDir, logFileExtension);

        if (logDirectory is not null)
            DeleteOldLogFiles(logDirectory, baseLogFileName, logFileExtension);

        _logFileWriter = new StreamWriter(logPath, append: true);
    }

    /// <summary>
    /// Creates a new <see cref="FileLoggerProvider" /> that writes log messages to the <see cref="DefaultLogFilePath" />
    /// </summary>
    /// <exception cref="Exception">
    /// Throws the same exceptions as <see cref="Directory.CreateDirectory" /> and <see cref="StreamWriter" />.
    /// </exception>
    public FileLoggerProvider() : this(DefaultLogFilePath)
    {
    }

    /// <summary>
    /// Deletes old log files, i.e. log files that exceed <see cref="MaxLogFiles" />.
    /// </summary>
    /// <param name="logDirectory">The directory where the log files are stored.</param>
    /// <param name="baseLogFileName">The base name of the log files.</param>
    /// <param name="logFileExtension">The extension of the log files.</param>
    private static void DeleteOldLogFiles(string logDirectory, string baseLogFileName, string logFileExtension = "log")
    {
        var files = Directory.GetFiles(logDirectory)
            .Where(path => path.StartsWith(baseLogFileName) && path.EndsWith($".{logFileExtension}"))
            .OrderByDescending(path => path)
            .Skip(MaxLogFiles);

        foreach (var file in files)
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to delete log file: {file}");
                Console.WriteLine(e);
            }
        }
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
