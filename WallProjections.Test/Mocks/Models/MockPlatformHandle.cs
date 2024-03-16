using Avalonia.Platform;

namespace WallProjections.Test.Mocks.Models;

/// <summary>
/// A mock of <see cref="IPlatformHandle" />
/// </summary>
public class MockPlatformHandle : IPlatformHandle
{
    public IntPtr Handle => IntPtr.Zero;
    public string HandleDescriptor => "MockPlatformHandle";
}
