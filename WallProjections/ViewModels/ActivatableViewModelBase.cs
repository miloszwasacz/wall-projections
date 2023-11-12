using System.Reactive.Disposables;
using ReactiveUI;

namespace WallProjections.ViewModels;

/// <summary>
/// A base class for viewmodels that need to be activated and deactivated
/// </summary>
public abstract class ActivatableViewModelBase : ViewModelBase, IActivatableViewModel
{
    /// <summary>
    /// A base class for viewmodels that need to be activated and deactivated
    /// </summary>
    protected ActivatableViewModelBase()
    {
        this.WhenActivated(disposables =>
        {
            OnStart();
            Disposable
                .Create(OnStop)
                .DisposeWith(disposables);
        });
    }

    /// <inheritdoc />
    public ViewModelActivator Activator { get; } = new();

    /// <summary>
    /// The method to be called when the viewmodel is activated
    /// </summary>
    protected abstract void OnStart();

    /// <summary>
    /// The method to be called when the viewmodel is deactivated
    /// </summary>
    protected abstract void OnStop();
}
