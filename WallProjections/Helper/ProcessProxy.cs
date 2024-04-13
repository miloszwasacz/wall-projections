using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using WallProjections.Helper.Interfaces;

namespace WallProjections.Helper;

/// <inheritdoc />
public class ProcessProxy : IProcessProxy
{
    /// <summary>
    /// A logger for this class.
    /// </summary>
    private readonly ILogger _logger;

    [ExcludeFromCodeCoverage]
    public ProcessProxy(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ProcessProxy>();
    }

    // ReSharper disable once ConvertIfStatementToReturnStatement
    /// <inheritdoc />
    [ExcludeFromCodeCoverage(Justification = "Is platform specific (especially Linux) and should be tested manually")]
    public string? GetFileExplorerCommand()
    {
        //TODO Verify this works on all (necessary) platforms
        // Tested on: Windows 11

        if (OperatingSystem.IsWindows())
            return "explorer.exe";
        if (OperatingSystem.IsMacOS())
            return "open";
        if (OperatingSystem.IsLinux())
            return "pcmanfm"; // This only handles default Raspbian file manager

        return null;
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage(Justification = "Unit tests should not start external processes")]
    public void Start(string fileName, string arguments)
    {
        _logger.LogInformation("Starting process: {FileName} {Arguments}", fileName, arguments);
        Process.Start(fileName, arguments);
    }
}
