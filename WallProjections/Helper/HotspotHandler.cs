using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WallProjections.Helper.Interfaces;

namespace WallProjections.Helper;

/// <inheritdoc cref="IHotspotHandler" />
public sealed class HotspotHandler : IHotspotHandler, IDisposable
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
    /// A <see cref="IPythonHandler" /> which notifies when a hotspot is pressed or released.
    /// </summary>
    private readonly IPythonHandler _pythonHandler;

    /// <summary>
    /// A mutex to ensure thread safety for <see cref="_currentTask" /> and <see cref="_cancelledTasks" />.
    /// </summary>
    private readonly Mutex _taskMutex = new();

    /// <summary>
    /// The ID of the hotspot that is currently activated.
    /// </summary>
    private int? _currentlyActivated;

    /// <summary>
    /// The currently running task for activating a hotspot.
    /// </summary>
    /// <seealso cref="OnHotspotPressed" />
    /// <seealso cref="OnHotspotReleased" />
    private ActivationTask? _currentTask;

    /// <summary>
    /// The activation tasks that have been cancelled and can potentially be resumed.
    /// </summary>
    /// <seealso cref="OnHotspotPressed" />
    /// <seealso cref="OnHotspotReleased" />
    private readonly ConcurrentDictionary<int, ActivationTask> _cancelledTasks = new();

    /// <summary>
    /// Creates a new instance of <see cref="HotspotHandler" />
    /// </summary>
    /// <param name="pythonHandler">
    /// A <see cref="IPythonHandler" /> which notifies when a hotspot is pressed or released
    /// </param>
    public HotspotHandler(IPythonHandler pythonHandler)
    {
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
        _taskMutex.WaitOne();
        var runningTask = _currentTask;

        // If there is a running task which is not done, or the currently activated hotspot has been pressed, return
        if (_currentlyActivated == e.Id || (runningTask is not null && !runningTask.Done))
        {
            _taskMutex.ReleaseMutex();
            return;
        }

        // If the same task has been previously cancelled, resume that task; otherwise, create a new task
        _currentTask = _cancelledTasks.TryRemove(e.Id, out var cancelledTask)
            ? new ActivationTask(cancelledTask, InvokeActivating, InvokeActivatedAndUpdateCurrent)
            : new ActivationTask(e.Id, InvokeActivating, InvokeActivatedAndUpdateCurrent);

        // If there was a running task, it must have already finished, so remove it
        if (runningTask is not null)
            _cancelledTasks.TryRemove(runningTask.Id, out _);

        _taskMutex.ReleaseMutex();
    }

    //TODO Mark _currentlyActivated as null only when the screen is actually cleared
    /// <summary>
    /// An event callback for when a hotspot is <see cref="IPythonHandler.HotspotReleased">released</see>.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments containing the ID of the released hotspot.</param>
    private void OnHotspotReleased(object? sender, IHotspotHandler.HotspotArgs e)
    {
        _taskMutex.WaitOne();
        var currentTask = _currentTask;

        // If there is no running task or the task is done or the released hotspot is not the same as the current task, return
        if (currentTask is null || currentTask.Done || currentTask.Id != e.Id)
        {
            _taskMutex.ReleaseMutex();
            return;
        }

        // Cancel the current task and add it to the cancelled tasks
        currentTask.Cancel();
        _currentTask = null;
        _cancelledTasks.AddOrUpdate(currentTask.Id, currentTask, (_, _) => currentTask);

        if (_currentlyActivated is null || _currentlyActivated != e.Id)
            InvokeDeactivating(e.Id);

        _taskMutex.ReleaseMutex();
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
    /// This method uses <see cref="_taskMutex" /> to ensure thread safety. <b>Beware of deadlocks!</b>
    /// </remarks>
    private void InvokeActivatedAndUpdateCurrent(int id)
    {
        _taskMutex.WaitOne();

        // If there is a currently activated hotspot, deactivate it
        if (_currentlyActivated is not null)
            InvokeForcefullyDeactivated(_currentlyActivated.Value);

        // Activate the new hotspot
        _currentlyActivated = id;
        HotspotActivated?.Invoke(this, new IHotspotHandler.HotspotArgs(id));

        _taskMutex.ReleaseMutex();
    }

    // /// <summary>
    // /// Invokes the <see cref="HotspotActivated" /> event.
    // /// </summary>
    // /// <param name="id">The ID of the hotspot that is activated.</param>
    // private void InvokeActivated(int id) => HotspotActivated?.Invoke(this, new IHotspotHandler.HotspotArgs(id));

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
    private record ActivationTask
    {
        /// <summary>
        /// A mutex to ensure thread safety.
        /// </summary>
        private readonly Mutex _mutex = new();

        /// <summary>
        /// The cancellation token source for the task.
        /// </summary>
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
                _mutex.WaitOne();
                var done = _done;
                _mutex.ReleaseMutex();
                return done;
            }
        }

        /// <summary>
        /// Creates a new task which activates a hotspot.
        /// </summary>
        /// <param name="id">The ID of the hotspot to activate.</param>
        /// <param name="activatingEvent">The event to invoke when the hotspot starts activating.</param>
        /// <param name="activatedEvent">The event to invoke when the hotspot is fully activated.</param>
        /// <seealso cref="Run" />
        public ActivationTask(
            int id,
            Action<int> activatingEvent,
            Action<int> activatedEvent
        )
        {
            Id = id;
            Run(activatingEvent, activatedEvent);
        }

        /// <summary>
        /// Creates a new task which "resumes" the provided cancelled task.
        /// </summary>
        /// <param name="cancelledTask">Task to resume.</param>
        /// <param name="activatingEvent">The event to invoke when the hotspot starts activating.</param>
        /// <param name="activatedEvent">The event to invoke when the hotspot is fully activated.</param>
        /// <exception cref="ArgumentException">Thrown when the provided task has not been cancelled.</exception>
        public ActivationTask(
            ActivationTask cancelledTask,
            Action<int> activatingEvent,
            Action<int> activatedEvent
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
            Run(activatingEvent, activatedEvent, progress);
        }

        /// <summary>
        /// Runs the activation task - starts the activation, waits for <see cref="IHotspotHandler.ActivationTime" />
        /// minus the provided progress, finishes the activation and marks the task as <see cref="Done" />.
        /// </summary>
        /// <param name="activatingEvent">The event to invoke when the hotspot starts activating</param>
        /// <param name="activatedEvent">The event to invoke when the hotspot is fully activated</param>
        /// <param name="progress">The progress of the task</param>
        private void Run(
            Action<int> activatingEvent,
            Action<int> activatedEvent,
            TimeSpan? progress = null
        ) => Task.Run(async () =>
        {
            _mutex.WaitOne();

            // If the task was cancelled while waiting for the mutex, return
            if (_cts.IsCancellationRequested)
            {
                _mutex.ReleaseMutex();
                return;
            }

            // Start the activation
            activatingEvent(Id);
            _mutex.ReleaseMutex();

            // Wait for the remaining time
            // - If the progress is null or negative, wait for the full activation time
            // - If the progress is less than the activation time, wait for the remaining time
            // - If the progress is greater than the activation time, the task is done
            if (progress is null || progress.Value < TimeSpan.Zero)
                await Task.Delay(IHotspotHandler.ActivationTime);
            else if (progress.Value < IHotspotHandler.ActivationTime)
                await Task.Delay(IHotspotHandler.ActivationTime - progress.Value);

            // If the task was cancelled during activation, return
            _mutex.WaitOne();
            if (_cts.IsCancellationRequested)
            {
                _mutex.ReleaseMutex();
                return;
            }

            // Mark the hotspot as activated and the task as done
            activatedEvent(Id);
            _done = true;
            _mutex.ReleaseMutex();
        }, _cts.Token);

        /// <summary>
        /// Cancels the task using <see cref="_cts" /> and sets <see cref="CancelledAt" />.
        /// </summary>
        public void Cancel()
        {
            _mutex.WaitOne();
            var now = DateTimeOffset.UtcNow;
            _cts.Cancel();
            CancelledAt = (now, now - _startTime);
            _mutex.ReleaseMutex();
        }
    }
}
