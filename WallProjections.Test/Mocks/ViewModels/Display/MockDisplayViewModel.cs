using ReactiveUI;
using WallProjections.Helper.Interfaces;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Display;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;

namespace WallProjections.Test.Mocks.ViewModels.Display;

/// <summary>
/// A mock of <see cref="DisplayViewModel" />
/// </summary>
public sealed class MockDisplayViewModel : ViewModelBase, IDisplayViewModel
{
    /// <summary>
    /// A stub for <see cref="Description" /> for testing purposes
    /// </summary>
    public const string DefaultDescription = "Default Description: ";

    /// <summary>
    /// The backing field for <see cref="SetConfigs" />
    /// </summary>
    private readonly List<IConfig> _configs = new();

    /// <summary>
    /// The backing field for <see cref="CurrentHotspotId" />
    /// </summary>
    private int _currentHotspotId = -1;

    /// <summary>
    /// The <see cref="INavigator" /> to use for navigation between views
    /// </summary>
    private readonly INavigator _navigator;

    /// <summary>
    /// Initializes a new <see cref="MockDisplayViewModel" /> with <see cref="ImageViewModel" /> set to
    /// <paramref name="imageViewModel" /> and <see cref="VideoViewModel" /> set to <paramref name="videoViewModel" />
    /// </summary>
    /// <param name="navigator"><see cref="INavigator" /> to use by the mock</param>
    /// <param name="imageViewModel"><see cref="IImageViewModel" /> to use by the mock</param>
    /// <param name="videoViewModel"><see cref="IVideoViewModel" /> to use by the mock</param>
    /// <remarks>
    /// Uses <see cref="MockImageViewModel" /> and <see cref="MockVideoViewModel" />
    /// if the respective parameters are <i>null</i>
    /// </remarks>
    public MockDisplayViewModel(
        INavigator? navigator = null,
        IImageViewModel? imageViewModel = null,
        IVideoViewModel? videoViewModel = null
    )
    {
        _navigator = navigator ?? new MockNavigator();
        ImageViewModel = imageViewModel ?? new MockImageViewModel();
        VideoViewModel = videoViewModel ?? new MockVideoViewModel();
    }

    // /// <summary>
    // /// The ID of an hotspot the viewmodel should display data about, or <i>-1</i> if no hotspot has ever been loaded
    // /// </summary>
    public int CurrentHotspotId
    {
        get => _currentHotspotId;
        private set
        {
            this.RaiseAndSetIfChanged(ref _currentHotspotId, value);
            this.RaisePropertyChanged(nameof(Description));
        }
    }

    /// <summary>
    /// The set of configs the viewmodel has been set to
    /// </summary>
    public IReadOnlyList<IConfig> SetConfigs => _configs;

    /// <summary>
    /// The number of times <see cref="Dispose" /> has been called
    /// </summary>
    public int DisposedCount { get; private set; }

    /// <summary>
    /// Adds <paramref name="value" /> to <see cref="SetConfigs" />
    /// </summary>
    public IConfig Config
    {
        set => _configs.Add(value);
    }

    /// <summary>
    /// Returns whether <see cref="SetConfigs" /> is not empty
    /// </summary>
    public bool IsContentLoaded => _configs.Count > 0;

    /// <summary>
    /// Returns <see cref="DefaultDescription" /> + the ID that the viewmodel was constructed with
    /// </summary>
    public string Description => DefaultDescription + CurrentHotspotId;

    /// <summary>
    /// Returns <see cref="MockImageViewModel" />
    /// </summary>
    public IImageViewModel ImageViewModel { get; }

    /// <summary>
    /// Returns a <see cref="MockVideoViewModel" />
    /// </summary>
    public IVideoViewModel VideoViewModel { get; }

    /// <summary>
    /// Sets <see cref="CurrentHotspotId" /> to <paramref name="e" />.<see cref="IPythonEventHandler.HotspotSelectedArgs.Id" />
    /// </summary>
    /// <param name="sender">The caller of the event (not used)</param>
    /// <param name="e">The event args containing the id of a hotspot to be theoretically loaded</param>
    public void OnHotspotSelected(object? sender, IPythonEventHandler.HotspotSelectedArgs e)
    {
        CurrentHotspotId = e.Id;
    }

    /// <summary>
    /// Calls <see cref="INavigator.OpenEditor" /> on <see cref="_navigator" />
    /// </summary>
    public void OpenEditor()
    {
        _navigator.OpenEditor();
    }

    /// <summary>
    /// Calls <see cref="INavigator.Shutdown" /> on <see cref="_navigator" />
    /// </summary>
    public void CloseDisplay()
    {
        _navigator.Shutdown();
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
