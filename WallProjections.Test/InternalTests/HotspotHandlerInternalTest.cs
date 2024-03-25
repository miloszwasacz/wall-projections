using WallProjections.Helper;

namespace WallProjections.Test.InternalTests;

[TestFixture]
public class HotspotHandlerInternalTest
{
    [TestFixture]
    public class ActivationTaskTest
    {
        [Test]
        public void ConstructorFromNotCancelledTaskTest()
        {
            var task = new HotspotHandler.ActivationTask(0, EmptyCallback, EmptyCallback);
            Assert.That(
                () => new HotspotHandler.ActivationTask(task, EmptyCallback, EmptyCallback),
                Throws.ArgumentException
            );
        }

        /// <summary>
        /// Tests if cancelling a task before running it works (i.e. does not start the activation)
        /// </summary>
        [Test]
        [Timeout(5000)]
        public async Task CancelledBeforeStartTest()
        {
            const int expectedId = -1;
            const int inputId = 0;
            var actualId = expectedId;

#pragma warning disable CS0618
            // Create a task without running it
            _ = new HotspotHandler.ActivationTask(
                inputId,
                id => actualId = id,
                out var runTask,
                out var cts
            );
#pragma warning restore CS0618

            Task task;
            lock (cts)
            {
                // Lock the internal mutex and run the task
                task = runTask();

                // Wait so that the task starts
                Task.Delay(100).Wait();

                // Cancel the task before starting the activation
                cts.Cancel();
            }

            // Assert that the task was cancelled without starting the activation
            await task;
            Assert.That(actualId, Is.EqualTo(expectedId));
        }

        /// <summary>
        /// An empty callback that takes an <i>int</i> does nothing
        /// </summary>
        private static void EmptyCallback(int _)
        {
        }
    }
}
