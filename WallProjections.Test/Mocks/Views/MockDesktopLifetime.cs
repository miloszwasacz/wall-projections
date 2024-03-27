using Avalonia.Controls.ApplicationLifetimes;

namespace WallProjections.Test.Mocks.Views;

/// <summary>
/// A mock implementation of <see cref="IClassicDesktopStyleApplicationLifetime" />
/// </summary>
public class MockDesktopLifetime : ClassicDesktopStyleApplicationLifetime, IClassicDesktopStyleApplicationLifetime
{
    // "Override" methods and properties as needed

    /// <summary>
    /// The backing field for <see cref="Shutdowns" />
    /// </summary>
    private readonly List<int> _shutdowns = new();

    /// <inheritdoc />
    public new event EventHandler<ControlledApplicationLifetimeExitEventArgs>? Exit;

    /// <summary>
    /// The exit codes passed to <see cref="Shutdown" />
    /// </summary>
    public IReadOnlyList<int> Shutdowns => _shutdowns;

    /// <summary>
    /// Adds <paramref name="exitCode" /> to <see cref="Shutdowns" />
    /// </summary>
    /// <param name="exitCode">The exit code to quit the application with</param>
    public new void Shutdown(int exitCode = 0)
    {
        _shutdowns.Add(exitCode);
        Exit?.Invoke(this, new ControlledApplicationLifetimeExitEventArgs(exitCode));
        MainWindow?.Close();
    }
}
