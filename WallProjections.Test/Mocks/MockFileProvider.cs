using WallProjections.Models.Interfaces;

namespace WallProjections.Test.Mocks;

public class MockFileProvider : IFileProvider
{
    //TODO Implement properly once FileProvider is refactored
    private readonly string[] _files;
    public string? FileNumber { get; private set; }

    public MockFileProvider(string[] files)
    {
        _files = files;
    }

    public string[] GetFiles(string fileNumber)
    {
        FileNumber = fileNumber;
        return _files;
    }
}
