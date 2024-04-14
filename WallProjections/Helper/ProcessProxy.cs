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
    private const string _pythonError = "Could not load Python virtual environment. Please check installation guide on website.";

    /// <summary>
    /// Folder to store virtual environment.
    /// </summary>
    private const string VirtualEnvFolder = "VirtualEnv";

    /// <summary>
    /// Full path to the Python virtual environment if it exists.
    /// </summary>
    private static string VirtualEnvPath => Path.Combine(IFileHandler.AppDataFolderPath, VirtualEnvFolder);

    private static string _venvLocatorScript => $"import sys;" +
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
    public (string, string) LoadPythonVirtualEnv()
    {
        Debug.WriteLine("Getting Python information from VirtualEnv...");

        string virtualEnvScriptsPath;
        if (OperatingSystem.IsWindows())
            virtualEnvScriptsPath = VirtualEnvPath + @"\Scripts";
        else
            virtualEnvScriptsPath = VirtualEnvPath + @"/bin";

        Console.WriteLine(virtualEnvScriptsPath);

        // Environment.SetEnvironmentVariable("PATH", pathVeScripts, EnvironmentVariableTarget.Process);

        // Process calls Python script to find location of
        var proc = new Process();
        proc.StartInfo.FileName = Path.Combine(virtualEnvScriptsPath, "python");
        proc.StartInfo.Arguments = $"-c \"{_venvLocatorScript}\"";
        proc.StartInfo.RedirectStandardOutput = true;

        if (!proc.Start())
            throw new Exception("Couldn't initialize Python in virtual environment");

        proc.WaitForExit();

        var rawOutput = proc.StandardOutput.ReadToEnd();

        if (string.IsNullOrEmpty(rawOutput))
            throw new Exception(_pythonError);

        var pythonOutput = rawOutput
            .Replace(Environment.NewLine, "")
            .Split(',');

        if (pythonOutput.Length != 2)
            throw new Exception(_pythonError);

        return (pythonOutput[0], pythonOutput[1]);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage(Justification = "Unit tests should not start external processes")]
    public void Start(string fileName, string arguments) => Process.Start(fileName, arguments);
}
