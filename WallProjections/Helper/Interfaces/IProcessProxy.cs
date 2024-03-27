using System.Diagnostics;

namespace WallProjections.Helper.Interfaces;

/// <summary>
/// A proxy for starting up <see cref="Process" />es.
/// </summary>
public interface IProcessProxy
{
    /// <returns>The command to open the file explorer <i>(OS dependent)</i>, or <i>null</i> if not supported.</returns>
    public string? GetFileExplorerCommand();

    /// <inheritdoc cref="Process.Start(string, string)" />
    public void Start(string fileName, string arguments);
}
