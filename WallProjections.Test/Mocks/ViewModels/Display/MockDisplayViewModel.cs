using System.Collections.Immutable;
using WallProjections.Helper.Interfaces;
using WallProjections.Models;
using WallProjections.Test.Mocks.ViewModels.Display.Layouts;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Display;
using WallProjections.ViewModels.Display.Layouts;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.Test.Mocks.ViewModels.Display;

/// <summary>
/// A mock of <see cref="DisplayViewModel" />
/// </summary>
public sealed class MockDisplayViewModel : ViewModelBase, IDisplayViewModel
{
    /// <summary>
    /// The <see cref="INavigator" /> to use for navigation between views
    /// </summary>
    private readonly INavigator _navigator;

    /// <summary>
    /// The <see cref="MockViewModelProvider" /> to pass to <see cref="MockLayoutProvider.GetLayout"/>
    /// </summary>
    private readonly MockViewModelProvider _viewModelProvider = new();

    /// <summary>
    /// The <see cref="MockLayoutProvider" /> to use for loading layouts
    /// </summary>
    private readonly MockLayoutProvider _layoutProvider = new();

    /// <summary>
    /// Initializes a new <see cref="MockDisplayViewModel" />
    /// that uses the provided <see cref="INavigator" /> for navigation,
    /// and a <see cref="MockLayoutProvider" /> for loading layouts.
    /// </summary>
    /// <param name="navigator"><see cref="INavigator" /> to use by the mock</param>
    /// <remarks>
    /// Uses <see cref="MockNavigator" /> if the provided parameter is <i>null</i>
    /// </remarks>
    public MockDisplayViewModel(INavigator? navigator = null)
    {
        _navigator = navigator ?? new MockNavigator();
        ContentViewModel = new WelcomeViewModel();
    }

    /// <summary>
    /// The number of times <see cref="Dispose" /> has been called
    /// </summary>
    public int DisposedCount { get; private set; }

    /// <inheritdoc />
    public void OnHotspotActivated(object? sender, IHotspotHandler.HotspotArgs e)
    {
        var media = new Hotspot.Media(
            e.Id,
            $"Hotspot {e.Id}",
            "",
            ImmutableList<string>.Empty,
            ImmutableList<string>.Empty
        );
        ContentViewModel = _layoutProvider.GetLayout(_viewModelProvider, media);
    }

    /// <inheritdoc />
    public Layout ContentViewModel { get; private set; }

    /// <summary>
    /// Calls <see cref="INavigator.OpenEditor" /> on <see cref="_navigator" />
    /// </summary>
    public Task OpenEditor()
    {
        _navigator.OpenEditor();
        return Task.CompletedTask;
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
        DisposedCount++;
    }
}
