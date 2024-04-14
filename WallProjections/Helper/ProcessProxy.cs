using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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

    /// <summary>
    /// Error message if Python virtual environment cannot be loaded.
    /// </summary>
    private const string PythonErrorMessage =
        "Could not load Python virtual environment. Please check installation guide on website.";

    /// <summary>
    /// Script used to load the Python DLL location and the Python Path from the virtual environment.
    /// </summary>
    private static readonly string VEnvLocatorScript = $"import sys;" +
                                                       $"import find_libpython;" +
                                                       $"print(" +
                                                       $"  find_libpython.find_libpython(), " +
                                                       $"  '{Path.PathSeparator}'.join(sys.path), " +
                                                       $"  sep = ',' )";

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
    public (string, string) LoadPythonVirtualEnv(string virtualEnvPath)
    {
        _logger.LogInformation("Getting Python information from VirtualEnv.");

        var virtualEnvScriptsPath = virtualEnvPath + (
            OperatingSystem.IsWindows()
                ? @"\Scripts"
                : "/bin"
        );

        // Process calls Python script to find location of the Python DLL and the Python Path
        var proc = Process.Start(new ProcessStartInfo
        {
            FileName = Path.Combine(virtualEnvScriptsPath, "python"),
            Arguments = $"-c \"{VEnvLocatorScript}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true
        }) ?? throw new Exception(PythonErrorMessage);
        proc.WaitForExit();

        var rawOutput = proc.StandardOutput.ReadToEnd();
        if (string.IsNullOrEmpty(rawOutput))
            throw new Exception(PythonErrorMessage);

        _logger.LogTrace("Python output:\n{RawOutput}", rawOutput);
        var pythonOutput = rawOutput
            .Replace(Environment.NewLine, "")
            .Split(',');

        if (pythonOutput.Length != 2)
            throw new Exception(PythonErrorMessage);

        _logger.LogTrace("Python DLL: {PythonDll}, Python Path: {PythonPath}", pythonOutput[0], pythonOutput[1]);
        return (pythonOutput[0], pythonOutput[1]);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage(Justification = "Unit tests should not start external processes")]
    public void Start(string fileName, string arguments)
    {
        _logger.LogInformation("Starting process: {FileName} {Arguments}", fileName, arguments);
        Process.Start(fileName, arguments);
    }
}
