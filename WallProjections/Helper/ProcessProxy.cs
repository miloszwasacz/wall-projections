using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using WallProjections.Helper.Interfaces;
using WallProjections.Models.Interfaces;

namespace WallProjections.Helper;

/// <inheritdoc />
public class ProcessProxy : IProcessProxy
{
    /// <summary>
    /// Error message if Python virtual environment cannot be loaded.
    /// </summary>
    private const string PythonErrorMessage = "Could not load Python virtual environment. Please check installation guide on website.";

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
    [ExcludeFromCodeCoverage( Justification = "Unit tests should not start external processes")]
    public (string, string) LoadPythonVirtualEnv(string virtualEnvPath)
    {
        Debug.WriteLine("Getting Python information from VirtualEnv...");

        var virtualEnvScriptsPath = virtualEnvPath + (
            OperatingSystem.IsWindows()
                ? @"\Scripts"
                : "/bin"
        );

        // Process calls Python script to find location of
        var proc = new Process();
        proc.StartInfo.FileName = Path.Combine(virtualEnvScriptsPath, "python");
        proc.StartInfo.Arguments = $"-c \"{VEnvLocatorScript}\"";
        proc.StartInfo.RedirectStandardOutput = true;

        if (!proc.Start())
            throw new Exception(PythonErrorMessage);

        proc.WaitForExit();

        var rawOutput = proc.StandardOutput.ReadToEnd();

        if (string.IsNullOrEmpty(rawOutput))
            throw new Exception(PythonErrorMessage);

        var pythonOutput = rawOutput
            .Replace(Environment.NewLine, "")
            .Split(',');

        if (pythonOutput.Length != 2)
            throw new Exception(PythonErrorMessage);

        return (pythonOutput[0], pythonOutput[1]);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage(Justification = "Unit tests should not start external processes")]
    public void Start(string fileName, string arguments) => Process.Start(fileName, arguments);
}
