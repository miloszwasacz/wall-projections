using WallProjections.Helper.Interfaces;

namespace WallProjections.Test.Mocks.Helper;

/// <summary>
/// A mock of <see cref="IProcessProxy" /> for injecting into tests.
/// </summary>
public class MockProcessProxy : IProcessProxy
{
    private readonly OS? _os;

    /// <summary>
    /// The last call to <see cref="Start" />.
    /// </summary>
    public (string, string)? LastStart { get; private set; }

    /// <summary>
    /// Creates a new <see cref="MockProcessProxy" /> that uses the current operating system,
    /// or <i>null</i> if the operating system is not supported.
    /// </summary>
    /// <seealso cref="OS" />
    public MockProcessProxy()
    {
        if (OperatingSystem.IsWindows())
            _os = OS.Windows;
        else if (OperatingSystem.IsMacOS())
            _os = OS.MacOS;
        else if (OperatingSystem.IsLinux())
            _os = OS.Linux;
    }

    /// <summary>
    /// Creates a new <see cref="MockProcessProxy" /> that uses <paramref name="os" /> as the operating system.
    /// </summary>
    /// <param name="os">The operating system to use.</param>
    /// <seealso cref="GetFileExplorerCommand" />
    public MockProcessProxy(OS? os)
    {
        _os = os;
    }

    /// <inheritdoc />
    /// <remarks>Uses the <see cref="OS" /> passed in the <see cref="MockProcessProxy(OS?)">constrctor</see>.</remarks>
    public string? GetFileExplorerCommand()
    {
        return _os switch
        {
            OS.Windows => "explorer.exe",
            OS.MacOS => "open",
            OS.Linux => "pcmanfm",
            _ => null
        };
    }

    /// <summary>
    /// Virtual env only required when running the actual program, so not required on mock.
    /// </summary>
    public (string, string) LoadPythonVirtualEnv(string virtualEnvPath)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Sets <see cref="LastStart" /> to <paramref name="fileName" /> and <paramref name="arguments" />.
    /// </summary>
    public void Start(string fileName, string arguments)
    {
        LastStart = (fileName, arguments);
    }

    // ReSharper disable InconsistentNaming
    /// <summary>
    /// Supported operating systems.
    /// </summary>
    public enum OS
    {
        Windows,
        MacOS,
        Linux
    }
    // ReSharper restore InconsistentNaming
}
