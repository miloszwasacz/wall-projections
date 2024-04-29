using System.Collections.Immutable;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks;
using WallProjections.Test.Mocks.Helper;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.ViewModels.SecondaryScreens;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;
using WallProjections.ViewModels.SecondaryScreens;

namespace WallProjections.Test.ViewModels.SecondaryScreens;

[TestFixture]
public class HotspotDisplayViewModelTest
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// A mock viewmodel provider
    /// </summary>
    private static readonly MockViewModelProvider VMProvider = new();

    /// <summary>
    /// Creates a new config with 3 hotspots
    /// </summary>
    private static IConfig CreateConfig()
    {
        var hotspots = new[]
        {
            new Coord(10, 10, 10),
            new Coord(50, 50, 14),
            new Coord(100, 200, 20)
        };
        return new Config(new double[3, 3], hotspots.Select((coord, i) => new Hotspot(
            i,
            coord,
            "",
            "",
            ImmutableList<string>.Empty,
            ImmutableList<string>.Empty)
        ));
    }

    [Test]
    public void ConstructorTest()
    {
        var config = CreateConfig();
        var hotspotHandler = new MockHotspotHandler();
        var hotspotViewModel = new HotspotDisplayViewModel(config, hotspotHandler, VMProvider, new MockLoggerFactory());
        Assert.Multiple(() =>
        {
            Assert.That(
                hotspotViewModel.Projections,
                Is.EquivalentTo(config.Hotspots).Using<AbsHotspotProjectionViewModel, Hotspot>(
                    (actual, expected) => actual.IsSameAsHotspot(expected)
                )
            );
            //TODO Add this assertion when the hiding has been properly implemented
            // Assert.That(hotspotViewModel.IsVisible, Is.False);
        });
    }

    [Test]
    [Timeout(2500)]
    public async Task ActivateDeactivateHotspotsTest()
    {
        var config = CreateConfig();
        var hotspotHandler = new MockHotspotHandler();
        var hotspotViewModel = new HotspotDisplayViewModel(config, hotspotHandler, VMProvider, new MockLoggerFactory());

        // Set activating
        hotspotHandler.StartHotspotActivation(0);
        await Task.Delay(50);
        AssertActivatingHotspot(hotspotViewModel, 0);

        // Set activated
        hotspotHandler.ActivateHotspot(0);
        await Task.Delay(50);
        AssertActiveHotspot(hotspotViewModel, 0);

        // Set deactivating
        hotspotHandler.DeactivateHotspot(0);
        await Task.Delay(50);
        AssertDeactivatingHotspot(hotspotViewModel, 0);

        // Forcefully deactivate
        hotspotHandler.ActivateHotspot(0);
        await Task.Delay(50);
        hotspotHandler.ForcefullyDeactivateHotspot(0);
        await Task.Delay(50);
        AssertActiveHotspot(hotspotViewModel, null);

        // Activate another hotspot
        hotspotHandler.ActivateHotspot(1);
        await Task.Delay(50);
        AssertActiveHotspot(hotspotViewModel, 1);

        // Deactivate all hotspots
        hotspotViewModel.DeactivateHotspots();
        AssertActiveHotspot(hotspotViewModel, null);
    }

    [Test]
    [Timeout(2500)]
    public async Task ActivateDeactivateNonexistentHotspotTest()
    {
        var config = CreateConfig();
        var hotspotHandler = new MockHotspotHandler();
        var hotspotViewModel = new HotspotDisplayViewModel(config, hotspotHandler, VMProvider, new MockLoggerFactory());

        // Set activating
        hotspotHandler.StartHotspotActivation(3);
        await Task.Delay(50);
        AssertActivatingHotspot(hotspotViewModel, null);

        // Set activated
        hotspotHandler.ActivateHotspot(3);
        await Task.Delay(50);


        // Activate hotspot 0 to test deactivating nonexistent hotspot
        hotspotHandler.ActivateHotspot(0);
        await Task.Delay(50);


        // Set deactivating
        hotspotHandler.DeactivateHotspot(3);
        await Task.Delay(50);
        AssertActiveHotspot(hotspotViewModel, 0);

        // Forcefully deactivate
        hotspotHandler.ForcefullyDeactivateHotspot(3);
        await Task.Delay(50);
        AssertActiveHotspot(hotspotViewModel, 0);
    }

    //TODO Enable this test when the hiding has been properly implemented
    [Test]
    [Ignore("Hiding has not yet been properly implemented")]
    public void DisplayHotspotsTest()
    {
        var hotspotHandler = new MockHotspotHandler();
        var hotspotViewModel = new HotspotDisplayViewModel(CreateConfig(), hotspotHandler, VMProvider, new MockLoggerFactory());
        Assert.That(hotspotViewModel.IsVisible, Is.False);
        hotspotViewModel.DisplayHotspots();
        Assert.That(hotspotViewModel.IsVisible, Is.True);
    }

    [Test]
    public void DisposeTest()
    {
        var hotspotHandler = new MockHotspotHandler();
        var hotspotViewModel = new HotspotDisplayViewModel(CreateConfig(), hotspotHandler, VMProvider, new MockLoggerFactory());
        AssertActiveHotspot(hotspotViewModel, null);

        hotspotViewModel.Dispose();

        // The event is ignored, so it should not have any effect
        hotspotHandler.ActivateHotspot(0);
        AssertActiveHotspot(hotspotViewModel, null);
    }

    /// <summary>
    /// Asserts that the activating hotspot is the one with the given id, and that all other hotspots are not activating.
    /// </summary>
    /// <param name="vm">The source of the hotspots.</param>
    /// <param name="activatingId">
    /// The id of the hotspot that should be activating. If <i>null</i>, no hotspots should be activating.
    /// </param>
    private static void AssertActivatingHotspot(AbsHotspotDisplayViewModel vm, int? activatingId)
    {
        AssertChangedHotspot(vm, activatingId, coord => coord.State == HotspotState.Activating);

        if (activatingId is null) return;
        var hotspot = vm.Projections.FirstOrDefault(coord => coord.Id == activatingId);
        Assert.Multiple(() =>
        {
            Assert.That(hotspot?.State, Is.Not.EqualTo(HotspotState.Active));
            Assert.That(hotspot?.State, Is.Not.EqualTo(HotspotState.Deactivating));
        });
    }

    /// <summary>
    /// Asserts that the active hotspot is the one with the given id, and that all other hotspots are inactive.
    /// </summary>
    /// <param name="vm">The source of the hotspots.</param>
    /// <param name="activeId">
    /// The id of the hotspot that should be active. If <i>null</i>, no hotspots should be active.
    /// </param>
    private static void AssertActiveHotspot(AbsHotspotDisplayViewModel vm, int? activeId)
    {
        AssertChangedHotspot(vm, activeId, coord => coord.State == HotspotState.Active);

        if (activeId is null) return;
        var hotspot = vm.Projections.FirstOrDefault(coord => coord.Id == activeId);
        Assert.Multiple(() =>
        {
            Assert.That(hotspot?.State, Is.Not.EqualTo(HotspotState.Activating));
            Assert.That(hotspot?.State, Is.Not.EqualTo(HotspotState.Deactivating));
        });
    }

    /// <summary>
    /// Asserts that the deactivating hotspot is the one with the given id, and that all other hotspots are not deactivating.
    /// </summary>
    /// <param name="vm">The source of the hotspots.</param>
    /// <param name="deactivatingId">
    /// The id of the hotspot that should be deactivating. If <i>null</i>, no hotspots should be deactivating.
    /// </param>
    public static void AssertDeactivatingHotspot(AbsHotspotDisplayViewModel vm, int? deactivatingId)
    {
        AssertChangedHotspot(vm, deactivatingId, coord => coord.State == HotspotState.Deactivating);

        if (deactivatingId is null) return;
        var hotspot = vm.Projections.FirstOrDefault(coord => coord.Id == deactivatingId);
        Assert.Multiple(() =>
        {
            Assert.That(hotspot?.State, Is.Not.EqualTo(HotspotState.Activating));
            Assert.That(hotspot?.State, Is.Not.EqualTo(HotspotState.Active));
        });
    }

    /// <summary>
    /// Asserts that there is only one or zero hotspots that satisfy the <paramref name="grouping" />.
    /// </summary>
    /// <param name="vm">The source of the hotspots.</param>
    /// <param name="activeId">
    /// The id of the hotspot that should be satisfy the <paramref name="grouping" />.
    /// If <i>null</i>, no hotspots should satisfy the <paramref name="grouping" />.
    /// </param>
    /// <param name="grouping">A function that determines whether a hotspot satisfies a condition.</param>
    private static void AssertChangedHotspot(
        AbsHotspotDisplayViewModel vm,
        int? activeId,
        Func<AbsHotspotProjectionViewModel, bool> grouping
    )
    {
        var projections = vm.Projections.GroupBy(grouping).ToImmutableList();
        var satisfying = projections.Where(g => g.Key).SelectMany(g => g).ToImmutableList();
        var notSatisfying = projections.Where(g => !g.Key).SelectMany(g => g).ToImmutableList();
        var expectedActiveCount = activeId is null ? 0 : 1;
        Assert.Multiple(() =>
        {
            Assert.That(satisfying, Has.Count.EqualTo(expectedActiveCount));
            Assert.That(notSatisfying, Has.Count.EqualTo(vm.Projections.Count() - expectedActiveCount));
        });
        if (activeId is not null)
            Assert.That(satisfying[0].Id, Is.EqualTo(activeId));
    }
}
