using WallProjections.Helper;
using WallProjections.Helper.Interfaces;
using WallProjections.Test.Mocks.Helper;
using HotspotArgs = WallProjections.Helper.Interfaces.IHotspotHandler.HotspotArgs;

namespace WallProjections.Test.Helper;

[TestFixture]
public class HotspotHandlerTest
{
    /// <summary>
    /// Test ids of hotspots
    /// </summary>
    private static readonly int[] Ids = { 0, 1, 2, 1000, -100 };

    [Test]
    public void ConstructorTest()
    {
        var pythonHandler = new MockPythonHandler();
        using var hotspotHandler = new HotspotHandler(pythonHandler);

        Assert.Multiple(() =>
        {
            Assert.That(hotspotHandler, Is.Not.Null);
            Assert.That(pythonHandler.HasPressedSubscribers);
            Assert.That(pythonHandler.HasReleasedSubscribers);
        });
    }

    /// <summary>
    /// Tests the activation hotspots:
    /// <ul>
    ///   <li>Asserts that the <see cref="HotspotState.Activating" /> event is fired when a hotspot is pressed</li>
    ///   <li>Asserts that no other hotspot can be activated while one is activating</li>
    ///   <li>Asserts that the <see cref="HotspotState.Activated" /> event is fired when a hotspot is fully activated</li>
    ///   <li>Asserts that the next hotspot can be activated after the first one is fully activated</li>
    /// </ul>
    /// </summary>
    [Test]
    [Timeout(20000)]
    public async Task ActivationTest()
    {
        var pythonHandler = new MockPythonHandler();
        using var hotspotHandler = new HotspotHandler(pythonHandler);

        await AssertCallbacks(hotspotHandler, async getEventArgs =>
        {
            // Assert that the first hotspot is activating
            pythonHandler.OnHotspotPressed(Ids[0]);
            await Task.Delay(100);
            var args1 = getEventArgs(HotspotState.Activating);
            Assert.That(args1, Is.Not.Null);
            Assert.That(args1!.Id, Is.EqualTo(Ids[0]));

            // Assert that the second hotspot cannot be activated before the first one is fully activated
            pythonHandler.OnHotspotPressed(Ids[1]);
            await Task.Delay(100);
            var args2 = getEventArgs(HotspotState.Activating);
            Assert.That(args2, Is.Null);

            // Assert that the first hotspot is fully activated
            await Task.Delay(IHotspotHandler.ActivationTime);
            var args3 = getEventArgs(HotspotState.Activated);
            Assert.That(args3, Is.Not.Null);
            Assert.That(args3!.Id, Is.EqualTo(Ids[0]));

            // Assert that the second hotspot can now be activated
            pythonHandler.OnHotspotPressed(Ids[1]);
            await Task.Delay(100);
            var args4 = getEventArgs(HotspotState.Activating);
            Assert.That(args4, Is.Not.Null);
            Assert.That(args4!.Id, Is.EqualTo(Ids[1]));

            // Assert that the second hotspot is fully activated
            await Task.Delay(IHotspotHandler.ActivationTime);
            var args5 = getEventArgs(HotspotState.Activated);
            Assert.That(args5, Is.Not.Null);
            Assert.That(args5!.Id, Is.EqualTo(Ids[1]));

            // Assert that the first hotspot has been forcefully deactivated
            await Task.Delay(IHotspotHandler.ForcefulDeactivationTime);
            var args6 = getEventArgs(HotspotState.ForcefullyDeactivated);
            Assert.That(args6, Is.Not.Null);
            Assert.That(args6!.Id, Is.EqualTo(Ids[0]));
        });
    }

    /// <summary>
    /// Tests the deactivation of hotspots:
    /// <ul>
    ///   <li>Asserts that the <see cref="HotspotState.Deactivating" /> event is fired when a hotspot is released</li>
    ///   <li>Asserts that the next hotspot can be activated while the first one is deactivating</li>
    ///   <li>Asserts that the first hotspot's activation is resumed, not restarted, when reactivated</li>
    ///   <li>Asserts that releasing the first hotspot after it has been fully activated does nothing</li>
    /// </ul>
    /// </summary>
    [Test]
    [Timeout(20000)]
    public async Task DeactivationTest()
    {
        var pythonHandler = new MockPythonHandler();
        using var hotspotHandler = new HotspotHandler(pythonHandler);

        await AssertCallbacks(hotspotHandler, async getEventArgs =>
        {
            var eventDelay = TimeSpan.FromMilliseconds(100);

            // Start activating first hotspot
            pythonHandler.OnHotspotPressed(Ids[0]);
            await Task.Delay(IHotspotHandler.ActivationTime / 2);

            // Assert that the first hotspot starts deactivating
            pythonHandler.OnHotspotUnpressed(Ids[0]);
            await Task.Delay(eventDelay);
            var args1 = getEventArgs(HotspotState.Deactivating);
            Assert.That(args1, Is.Not.Null);
            Assert.That(args1!.Id, Is.EqualTo(Ids[0]));

            // Assert that the second hotspot can be activated while the first is deactivating
            pythonHandler.OnHotspotPressed(Ids[1]);
            await Task.Delay(eventDelay);
            var args2 = getEventArgs(HotspotState.Activating);
            Assert.That(args2, Is.Not.Null);
            Assert.That(args2!.Id, Is.EqualTo(Ids[1]));

            // Assert that the first hotspot cannot be reactivated while the second is activating
            pythonHandler.OnHotspotPressed(Ids[0]);
            await Task.Delay(eventDelay);
            var args3 = getEventArgs(HotspotState.Activating);
            Assert.That(args3, Is.Null);

            // Cancel the activation of the second hotspot
            pythonHandler.OnHotspotUnpressed(Ids[1]);
            await Task.Delay(eventDelay);
            var args4 = getEventArgs(HotspotState.Deactivating);
            Assert.That(args4, Is.Not.Null);
            Assert.That(args4!.Id, Is.EqualTo(Ids[1]));

            // Assert that the first hotspot's activation is resumed, not restarted
            pythonHandler.OnHotspotPressed(Ids[0]);
            await Task.Delay(IHotspotHandler.ActivationTime / 2);
            var args5 = getEventArgs(HotspotState.Activating);
            var args6 = getEventArgs(HotspotState.Activated);
            Assert.Multiple(() =>
            {
                // Assert that the first hotspot is still activating because
                // some time has passed between the deactivation and reactivation
                Assert.That(args5, Is.Not.Null);
                Assert.That(args6, Is.Null);
            });
            Assert.That(args5!.Id, Is.EqualTo(Ids[0]));
            await Task.Delay(5 * eventDelay); // Wait for the rest of the activation time (plus a little extra)
            var args7 = getEventArgs(HotspotState.Activated);
            Assert.That(args7, Is.Not.Null);
            Assert.That(args7!.Id, Is.EqualTo(Ids[0]));

            // Assert that releasing the first hotspot after it has been fully activated does nothing
            pythonHandler.OnHotspotUnpressed(Ids[0]);
            await Task.Delay(eventDelay);
            var args8 = getEventArgs(HotspotState.Deactivating);
            Assert.That(args8, Is.Null);
        });
    }

    /// <summary>
    /// Tests different operations on invalid input:
    /// <ul>
    ///   <li>Asserts that releasing a hotspot that was never activated does nothing</li>
    ///   <li>Asserts that releasing a hotspot that is not currently activating does nothing</li>
    ///   <li>Asserts that activating the same hotspot while it is activating does nothing</li>
    /// </ul>
    /// </summary>
    [Test]
    [Timeout(20000)]
    public async Task InvalidHotspotsTest()
    {
        var pythonHandler = new MockPythonHandler();
        using var hotspotHandler = new HotspotHandler(pythonHandler);

        await AssertCallbacks(hotspotHandler, async getEventArgs =>
        {
            // Release a hotspot that was never activated
            pythonHandler.OnHotspotUnpressed(Ids[0]);
            await Task.Delay(100);
            var args1 = getEventArgs(HotspotState.Deactivating);
            Assert.That(args1, Is.Null);

            // Release a hotspot that is not currently activating
            pythonHandler.OnHotspotPressed(Ids[0]);
            await Task.Delay(100);
            pythonHandler.OnHotspotUnpressed(Ids[1]);
            await Task.Delay(100);
            var args2 = getEventArgs(HotspotState.Activating);
            var args3 = getEventArgs(HotspotState.Deactivating);
            Assert.Multiple(() =>
            {
                Assert.That(args2, Is.Not.Null);
                Assert.That(args3, Is.Null);
            });
            Assert.That(args2!.Id, Is.EqualTo(Ids[0]));

            // Activate a the same hotspot while it is activating
            pythonHandler.OnHotspotPressed(Ids[0]);
            await Task.Delay(100);
            var args4 = getEventArgs(HotspotState.Activating);
            Assert.That(args4, Is.Null);
        });
    }

    /// <summary>
    /// Asserts that the different callbacks are fired correctly.
    /// </summary>
    /// <param name="hotspotHandler">The <see cref="HotspotHandler" /> to test.</param>
    /// <param name="assertions">
    /// The assertions to make (asynchronously). It provides a function that can be used to query the
    /// last event args of a specific event type. After querying, the event args are reset to <i>null</i>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an invalid <see cref="HotspotState" /> is queried.
    /// </exception>
    private static async Task AssertCallbacks(
        IHotspotHandler hotspotHandler,
        Func<Func<HotspotState, HotspotArgs?>, Task> assertions
    )
    {
        HotspotArgs?
            activatingResult = null,
            activatedResult = null,
            deactivatingResult = null,
            forceDeactivatedResult = null;

        // Attach callbacks
        hotspotHandler.HotspotActivating += (_, args) => activatingResult = args;
        hotspotHandler.HotspotActivated += (_, args) => activatedResult = args;
        hotspotHandler.HotspotDeactivating += (_, args) => deactivatingResult = args;
        hotspotHandler.HotspotForcefullyDeactivated += (_, args) => forceDeactivatedResult = args;

        await assertions(queried =>
        {
            HotspotArgs? args;
            switch (queried)
            {
                case HotspotState.Activating:
                    args = activatingResult;
                    activatingResult = null;
                    return args;
                case HotspotState.Activated:
                    args = activatedResult;
                    activatedResult = null;
                    return args;
                case HotspotState.Deactivating:
                    args = deactivatingResult;
                    deactivatingResult = null;
                    return args;
                case HotspotState.ForcefullyDeactivated:
                    args = forceDeactivatedResult;
                    deactivatingResult = null;
                    return args;
                default:
                    throw new ArgumentOutOfRangeException(nameof(queried), queried, null);
            }
        });
    }

    /// <summary>
    /// The different callbacks that can be fired by the <see cref="HotspotHandler" />
    /// </summary>
    /// <seealso cref="HotspotHandlerTest.AssertCallbacks" />
    private enum HotspotState
    {
        Activating,
        Activated,
        Deactivating,
        ForcefullyDeactivated
    }
}
