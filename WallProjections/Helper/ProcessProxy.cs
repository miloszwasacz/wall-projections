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
    /// Error message if Python environment cannot be loaded.
    /// </summary>
    private const string PythonErrorMessage =
        "Could not load Python environment. Please check installation guide on website.";

    /// <summary>
    /// Script used to load the Python DLL location and the Python Path from the Python environment.
    /// </summary>
    private static readonly string PyEnvLocatorScript = $"import sys;" +
                                                       $"import find_libpython;" +
                                                       $"print(" +
                                                       $"  find_libpython.find_libpython(), " +
                                                       $"  '{Path.PathSeparator}'.join(sys.path), " +
                                                       $"  sep = ',' )";

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
        // Tested on:
        // - Windows 11
        // - macOS 14.4.1
        // - Ubuntu 22.04

        if (OperatingSystem.IsWindows())
            return "explorer.exe";
        if (OperatingSystem.IsMacOS())
            return "open";
        if (OperatingSystem.IsLinux())
            return "xdg-open";

        return null;
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage(Justification = "Unit tests should not start external processes")]
    public (string, string) LoadPythonEnv(string pythonExecutablePath)
    {
        _logger.LogInformation("Getting Python information from environment.");

        // Process calls Python script to find location of the Python DLL and the Python Path
        var proc = Process.Start(new ProcessStartInfo
        {
            FileName = pythonExecutablePath,
            Arguments = $"-c \"{PyEnvLocatorScript}\"",
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
