using Microsoft.Extensions.Logging;

namespace WallProjections.Test.Mocks;

/// <summary>
/// A mock logger that writes to the console.
/// </summary>
public class MockLogger : ILogger
{
    /// <summary>
    /// Whether the logger should write to the console.
    /// </summary>
    private readonly bool _enabled;

    public MockLogger(bool enabled)
    {
        _enabled = enabled;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        if (!_enabled) return;

        if (logLevel is LogLevel.Critical or LogLevel.Error)
            Console.Error.WriteLine(formatter(state, exception));
        else
            Console.WriteLine(formatter(state, exception));
    }

    public bool IsEnabled(LogLevel logLevel) => _enabled;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
}

/// <summary>
/// A mock logger factory that creates <see cref="MockLogger" />s.
/// </summary>
public sealed class MockLoggerFactory : ILoggerFactory
{
    /// <summary>
    /// Whether the logger should write to the console.
    /// </summary>
    public bool Enabled { get; set; }

    public MockLoggerFactory(bool enabled = true)
    {
        Enabled = enabled;
    }

    public ILogger CreateLogger(string categoryName) => new MockLogger(Enabled);

    public void AddProvider(ILoggerProvider provider)
    {
    }

    public void Dispose()
    {
    }
}
