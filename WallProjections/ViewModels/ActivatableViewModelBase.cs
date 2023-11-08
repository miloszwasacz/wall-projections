using System.Reactive.Disposables;
using ReactiveUI;

namespace WallProjections.ViewModels;

public abstract class ActivatableViewModelBase : ViewModelBase, IActivatableViewModel
{
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

    public ViewModelActivator Activator { get; } = new();

    protected abstract void OnStart();
    protected abstract void OnStop();
}
