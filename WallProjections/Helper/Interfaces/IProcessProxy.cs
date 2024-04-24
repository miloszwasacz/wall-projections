using System.Diagnostics;

namespace WallProjections.Helper.Interfaces;

/// <summary>
/// A proxy for starting up <see cref="Process" />es.
/// </summary>
public interface IProcessProxy
{
    /// <returns>The command to open the file explorer <i>(OS dependent)</i>, or <i>null</i> if not supported.</returns>
    public string? GetFileExplorerCommand();

    /// <summary>
    /// Gets the full path and the DLL for the Python virtual environment.
    /// </summary>
    /// <param name="pythonExecutablePath">Path where the virtual environment is located.</param>
    /// <returns>
    ///     (Python DLL - used to run PythonNET, Python Path - location of all executable files accessible to python)
    ///     of virtual environment stored at <see cref="pythonExecutablePath"/>
    /// </returns>
    /// <exception>If the virtual environment cannot be loaded for PythonNET</exception>
    public (string, string) LoadPythonEnv(string pythonExecutablePath);

    /// <inheritdoc cref="Process.Start(string, string)" />
    public void Start(string fileName, string arguments);
}
