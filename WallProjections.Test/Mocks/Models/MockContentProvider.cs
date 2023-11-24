using WallProjections.Models;
using WallProjections.Models.Interfaces;

namespace WallProjections.Test.Mocks.Models;

public class MockContentProvider : IContentProvider
{
    private readonly List<Hotspot.Media> _media;

    //TODO Implement properly once ContentProvider is refactored
    public int? FileNumber { get; private set; }

    public MockContentProvider(List<Hotspot.Media> result)
    {
        _media = result;
    }

    public Hotspot.Media GetMedia(int hotspotId)
    {
        FileNumber = hotspotId;
        return _media.Find(media => media.Hotspot.Id == hotspotId) ??
               throw new IConfig.HotspotNotFoundException(hotspotId);
    }
}
