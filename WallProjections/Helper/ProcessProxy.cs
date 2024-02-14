using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using WallProjections.Helper.Interfaces;

namespace WallProjections.Helper;

/// <inheritdoc />
public class ProcessProxy : IProcessProxy
{
    // ReSharper disable once ConvertIfStatementToReturnStatement
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
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
    [ExcludeFromCodeCoverage]
    public void Start(string fileName, string arguments) => Process.Start(fileName, arguments);
}
