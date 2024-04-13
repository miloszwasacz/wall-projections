using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WallProjections.Helper.Interfaces;

namespace WallProjections.Helper;

/// <inheritdoc cref="IHotspotHandler" />
public sealed class HotspotHandler : IHotspotHandler
{
    /// <inheritdoc />
    public event EventHandler<IHotspotHandler.HotspotArgs>? HotspotActivating;

    /// <inheritdoc />
    public event EventHandler<IHotspotHandler.HotspotArgs>? HotspotActivated;

    /// <inheritdoc />
    public event EventHandler<IHotspotHandler.HotspotArgs>? HotspotDeactivating;

    /// <inheritdoc />
    public event EventHandler<IHotspotHandler.HotspotArgs>? HotspotForcefullyDeactivated;

    /// <summary>
    /// A logger for this class.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// A <see cref="IPythonHandler" /> which notifies when a hotspot is pressed or released.
    /// </summary>
    private readonly IPythonHandler _pythonHandler;

    /// <summary>
    /// The ID of the hotspot that is currently activated.
    /// </summary>
    /// <remarks>Whenever thread safety is required, use a <i>lock</i> around <see cref="_cancelledTasks" />.</remarks>
    private int? _currentlyActivated;

    /// <summary>
    /// The currently running task for activating a hotspot.
    /// </summary>
    /// <remarks>Whenever thread safety is required, use a <i>lock</i> around <see cref="_cancelledTasks" />.</remarks>
    /// <seealso cref="OnHotspotPressed" />
    /// <seealso cref="OnHotspotReleased" />
    private ActivationTask? _currentTask;

    /// <summary>
    /// The activation tasks that have been cancelled and can potentially be resumed.
    /// </summary>
    /// <remarks>Whenever thread safety is required, use a <i>lock</i> around this object.</remarks>
    /// <seealso cref="OnHotspotPressed" />
    /// <seealso cref="OnHotspotReleased" />
    private readonly ConcurrentDictionary<int, ActivationTask> _cancelledTasks = new();

    /// <summary>
    /// Creates a new instance of <see cref="HotspotHandler" />
    /// </summary>
    /// <param name="pythonHandler">
    /// A <see cref="IPythonHandler" /> which notifies when a hotspot is pressed or released
    /// </param>
    /// <param name="loggerFactory">A factory for creating loggers</param>
    public HotspotHandler(IPythonHandler pythonHandler, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<HotspotHandler>();
        _pythonHandler = pythonHandler;
        _pythonHandler.HotspotPressed += OnHotspotPressed;
        _pythonHandler.HotspotReleased += OnHotspotReleased;
    }

    /// <summary>
    /// An event callback for when a hotspot is <see cref="IPythonHandler.HotspotPressed">pressed</see>.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments containing the ID of the pressed hotspot.</param>
    private void OnHotspotPressed(object? sender, IHotspotHandler.HotspotArgs e)
    {
        lock (_cancelledTasks)
        {
            var runningTask = _currentTask;

            // If there is a running task which is not done, or the currently activated hotspot has been pressed, return
            if (_currentlyActivated == e.Id || (runningTask is not null && !runningTask.Done))
            {
                _logger.LogTrace("Ignoring hotspot {HotspotId} press", e.Id);
                return;
            }

            // If the same task has been previously cancelled, resume that task; otherwise, create a new task
            _currentTask = _cancelledTasks.TryRemove(e.Id, out var cancelledTask)
                ? new ActivationTask(cancelledTask, InvokeActivating, InvokeActivatedAndUpdateCurrent, _logger)
                : new ActivationTask(e.Id, InvokeActivating, InvokeActivatedAndUpdateCurrent, _logger);

            // If there was a running task, it must have already finished, so remove it
            if (runningTask is not null)
                _cancelledTasks.TryRemove(runningTask.Id, out _);
        }
    }

    //TODO Mark _currentlyActivated as null only when the screen is actually cleared
    /// <summary>
    /// An event callback for when a hotspot is <see cref="IPythonHandler.HotspotReleased">released</see>.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments containing the ID of the released hotspot.</param>
    private void OnHotspotReleased(object? sender, IHotspotHandler.HotspotArgs e)
    {
        lock (_cancelledTasks)
        {
            var currentTask = _currentTask;

            // If there is no running task or the task is done or the released hotspot is not the same as the current task, return
            if (currentTask is null || currentTask.Done || currentTask.Id != e.Id)
                return;

            // Cancel the current task and add it to the cancelled tasks
            currentTask.Cancel();
            _currentTask = null;
            _cancelledTasks.AddOrUpdate(
                currentTask.Id,
                currentTask,
                [ExcludeFromCodeCoverage(Justification = "This should never happen")]
                (_, _) => currentTask
            );

            if (_currentlyActivated is null || _currentlyActivated != e.Id)
                InvokeDeactivating(e.Id);
        }
    }

    /// <summary>
    /// Unsubscribes from the events of the <see cref="_pythonHandler" />.
    /// </summary>
    public void Dispose()
    {
        _pythonHandler.HotspotPressed -= OnHotspotPressed;
        _pythonHandler.HotspotReleased -= OnHotspotReleased;
    }

    #region Event Invokers

    /// <summary>
    /// Invokes the <see cref="HotspotActivating" /> event.
    /// </summary>
    /// <param name="id">The ID of the hotspot that is activating.</param>
    private void InvokeActivating(int id) => HotspotActivating?.Invoke(this, new IHotspotHandler.HotspotArgs(id));

    /// <summary>
    /// Invokes the <see cref="HotspotActivated" /> event and updates <see cref="_currentlyActivated" />.
    /// </summary>
    /// <param name="id">The ID of the hotspot that is activated.</param>
    /// <remarks>
    /// This method uses a <i>lock</i> around <see cref="_cancelledTasks" /> to ensure thread safety.
    /// <b>Beware of deadlocks!</b>
    /// </remarks>
    private void InvokeActivatedAndUpdateCurrent(int id)
    {
        lock (_cancelledTasks)
        {
            // If there is a currently activated hotspot, deactivate it
            if (_currentlyActivated is not null)
                InvokeForcefullyDeactivated(_currentlyActivated.Value);

            // Activate the new hotspot
            _currentlyActivated = id;
            HotspotActivated?.Invoke(this, new IHotspotHandler.HotspotArgs(id));
        }
    }

    /// <summary>
    /// Invokes the <see cref="HotspotDeactivating" /> event.
    /// </summary>
    /// <param name="id">The ID of the hotspot that is deactivating.</param>
    private void InvokeDeactivating(int id) => HotspotDeactivating?.Invoke(this, new IHotspotHandler.HotspotArgs(id));

    /// <summary>
    /// Invokes the <see cref="HotspotForcefullyDeactivated" /> event.
    /// </summary>
    /// <param name="id">The ID of the hotspot that is forcefully deactivated.</param>
    private void InvokeForcefullyDeactivated(int id) =>
        HotspotForcefullyDeactivated?.Invoke(this, new IHotspotHandler.HotspotArgs(id));

    #endregion

    /// <summary>
    /// A task that activates a hotspot after a certain amount of time.
    /// </summary>
    internal record ActivationTask
    {
        /// <summary>
        /// The cancellation token source for the task.
        /// </summary>
        /// <remarks>Whenever thread safety is required, use a <i>lock</i> around this object.</remarks>
        private readonly CancellationTokenSource _cts = new();

        /// <summary>
        /// The time when the task was started.
        /// </summary>
        private readonly DateTimeOffset _startTime = DateTimeOffset.UtcNow;

        /// <summary>
        /// The backing field for <see cref="Done" />
        /// </summary>
        private bool _done;

        /// <summary>
        /// The ID of the hotspot to activate.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// The time when the task was cancelled and the progress at that time.
        /// </summary>
        private (DateTimeOffset time, TimeSpan progress)? CancelledAt { get; set; }

        /// <summary>
        /// Whether the task has finished activating the hotspot.
        /// </summary>
        public bool Done
        {
            get
            {
                lock (_cts)
                {
                    return _done;
                }
            }
        }

        /// <summary>
        /// Creates a new task which activates a hotspot.
        /// </summary>
        /// <param name="id">The ID of the hotspot to activate.</param>
        /// <param name="activatingCallback">The callback to invoke when the hotspot starts activating.</param>
        /// <param name="activatedCallback">The callback to invoke when the hotspot is fully activated.</param>
        /// <param name="logger">A logger for debugging purposes.</param>
        /// <seealso cref="Run" />
        public ActivationTask(
            int id,
            Action<int> activatingCallback,
            Action<int> activatedCallback,
            ILogger logger
        )
        {
            Id = id;
            Run(activatingCallback, activatedCallback);
            logger.LogTrace("Started activation task for hotspot {HotspotId}", id);
        }

        /// <summary>
        /// Creates a new task which "resumes" the provided cancelled task.
        /// </summary>
        /// <param name="cancelledTask">Task to resume.</param>
        /// <param name="activatingCallback">The callback to invoke when the hotspot starts activating.</param>
        /// <param name="activatedCallback">The callback to invoke when the hotspot is fully activated.</param>
        /// <param name="logger">A logger for debugging purposes.</param>
        /// <exception cref="ArgumentException">Thrown when the provided task has not been cancelled.</exception>
        public ActivationTask(
            ActivationTask cancelledTask,
            Action<int> activatingCallback,
            Action<int> activatedCallback,
            ILogger logger
        )
        {
            // Cannot resume a task that has not been cancelled
            if (cancelledTask.CancelledAt is null)
                throw new ArgumentException("The cancelled task must have been cancelled");

            Id = cancelledTask.Id;

            // Calculate the new progress of the task:
            // - If the task was done, we start from the beginning
            // - If the task was not done, we subtract the time passed since the task was cancelled from the old progress
            var (cancelledAt, oldProgress) = cancelledTask.CancelledAt.Value;
            var progress = cancelledTask.Done
                ? TimeSpan.Zero
                : oldProgress - (DateTimeOffset.UtcNow - cancelledAt);

            // Run the task with the new progress
            Run(activatingCallback, activatedCallback, progress);
            logger.LogTrace(
                "Resumed activation task for hotspot {HotspotId} (progress: {Progress} ms)",
                Id,
                progress.Milliseconds
            );
        }

        /// <summary>
        /// Runs the activation task - starts the activation, waits for <see cref="IHotspotHandler.ActivationTime" />
        /// minus the provided progress, finishes the activation and marks the task as <see cref="Done" />.
        /// </summary>
        /// <param name="activatingCallback">The callback to invoke when the hotspot starts activating</param>
        /// <param name="activatedCallback">The callback to invoke when the hotspot is fully activated</param>
        /// <param name="progress">The progress of the task</param>
        private Task Run(
            Action<int> activatingCallback,
            Action<int> activatedCallback,
            TimeSpan? progress = null
        ) => Task.Run(async () =>
        {
            lock (_cts)
            {
                // If the task was cancelled while waiting for the lock, return
                if (_cts.IsCancellationRequested)
                    return;

                // Start the activation
                activatingCallback(Id);
            }

            // Wait for the remaining time
            // - If the progress is null or negative, wait for the full activation time
            // - If the progress is less than the activation time, wait for the remaining time
            // - If the progress is greater than the activation time, the task is done
            if (progress is null || progress.Value < TimeSpan.Zero)
                await Task.Delay(IHotspotHandler.ActivationTime);
            else if (progress.Value < IHotspotHandler.ActivationTime)
                await Task.Delay(IHotspotHandler.ActivationTime - progress.Value);

            lock (_cts)
            {
                // If the task was cancelled during activation, return
                if (_cts.IsCancellationRequested)
                    return;

                // Mark the hotspot as activated and the task as done
                activatedCallback(Id);
                _done = true;
            }
        }, _cts.Token);

        /// <summary>
        /// Cancels the task using <see cref="_cts" /> and sets <see cref="CancelledAt" />.
        /// </summary>
        public void Cancel()
        {
            lock (_cts)
            {
                var now = DateTimeOffset.UtcNow;
                _cts.Cancel();
                CancelledAt = (now, now - _startTime);
            }
        }

        /// <summary>
        /// This constructor is for testing purposes only.
        /// It does not run the activation task to allow checking if the task can be cancelled before it starts.
        /// </summary>
        /// <param name="id">The ID of the hotspot to activate.</param>
        /// <param name="activatingCallback">The callback to invoke when the hotspot starts activating.</param>
        /// <param name="runTask">The function to run the task with the provided <paramref name="activatingCallback" />.</param>
        /// <param name="cts">
        /// The cancellation token source used by the task. Use <i>lock</i> on this object to ensure thread safety.
        /// </param>
        [Obsolete("This constructor is for testing purposes only. See the documentation for more information.")]
        [ExcludeFromCodeCoverage(Justification = "This constructor is for testing purposes only.")]
        internal ActivationTask(
            int id,
            Action<int> activatingCallback,
            out Func<Task> runTask,
            out CancellationTokenSource cts
        )
        {
            Id = id;
            runTask = () => Run(activatingCallback, _ => { });
            cts = _cts;
        }
    }
}
