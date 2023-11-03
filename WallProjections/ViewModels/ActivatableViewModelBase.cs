using System.Reactive.Disposables;
using ReactiveUI;

namespace WallProjections.ViewModels;

public abstract class ActivatableViewModelBase : ViewModelBase, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();

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

    protected abstract void OnStart();
    protected abstract void OnStop();
}
