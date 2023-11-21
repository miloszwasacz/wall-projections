using WallProjections.Models.Interfaces;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.Test.Mocks.ViewModels;

/// <summary>
/// A mock of <see cref="DisplayViewModel"/>
/// </summary>
public sealed class MockDisplayViewModel : IDisplayViewModel
{
    /// <summary>
    /// A stub for <see cref="Description"/> for testing purposes
    /// </summary>
    public const string DefaultDescription = "Default Description: ";

    /// <summary>
    /// The backing field for <see cref="SetConfigs"/>
    /// </summary>
    private readonly List<IConfig> _configs = new();

    //TODO Add mention of ImageViewModel
    /// <summary>
    /// Initializes a new <see cref="MockDisplayViewModel" />
    /// with <see cref="VideoViewModel" /> set to <paramref name="videoViewModel" />
    /// (uses <see cref="MockVideoViewModel" /> if <paramref name="videoViewModel" /> is <i>null</i>)
    /// </summary>
    /// <param name="videoViewModel"><see cref="IVideoViewModel" /> to use by the mock</param>
    public MockDisplayViewModel(IVideoViewModel? videoViewModel = null)
    {
        VideoViewModel = videoViewModel ?? new MockVideoViewModel();
    }

    // /// <summary>
    // /// The ID of an hotspot the viewmodel should display data about, or <i>-1</i> if no hotspot has ever been loaded
    // /// </summary>
    public int CurrentHotspotId { get; private set; } = -1;

    /// <summary>
    /// The set of configs the viewmodel has been set to
    /// </summary>
    public IReadOnlyList<IConfig> SetConfigs => _configs;

    /// <summary>
    /// The number of times <see cref="Dispose"/> has been called
    /// </summary>
    public int DisposedCount { get; private set; }

    /// <summary>
    /// Adds <paramref name="value"/> to <see cref="SetConfigs"/>
    /// </summary>
    public IConfig Config
    {
        set => _configs.Add(value);
    }

    /// <summary>
    /// Returns <see cref="DefaultDescription"/> + the ID that the viewmodel was constructed with
    /// </summary>
    public string Description => DefaultDescription + CurrentHotspotId;

    /// <summary>
    /// Returns a <see cref="MockVideoViewModel"/>
    /// </summary>
    public IVideoViewModel VideoViewModel { get; }

    //TODO Implement mock properties
    public ImageViewModel ImageViewModel => throw new NotImplementedException();

    /// <summary>
    /// Sets <see cref="CurrentHotspotId" /> to <paramref name="hotspotId" />
    /// </summary>
    /// <param name="hotspotId">The id of a hotspot to be theoretically loaded</param>
    /// <returns><i>False</i> if <paramref name="hotspotId" /> = <i>-1</i>, <i>True</i> otherwise</returns>
    public bool LoadHotspot(int hotspotId)
    {
        CurrentHotspotId = hotspotId;
        return hotspotId != -1;
    }

    /// <summary>
    /// Calls <see cref="VideoViewModel.Dispose" /> on <see cref="VideoViewModel" />
    /// and increases <see cref="DisposedCount" />
    /// </summary>
    public void Dispose()
    {
        VideoViewModel.Dispose();
        DisposedCount++;
    }
}
