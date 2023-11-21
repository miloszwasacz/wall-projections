using WallProjections.Models;
using WallProjections.Models.Interfaces;

namespace WallProjections.Test.Mocks.Models;

public sealed class MockContentCache : IContentCache
{
    public IConfig Load(string zipPath)
    {
        throw new NotImplementedException();
    }

    public string GetHotspotMediaFolder(Hotspot hotspot)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
