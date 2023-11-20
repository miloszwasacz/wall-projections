using ReactiveUI;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.Test.Mocks.ViewModels;

/// <summary>
/// A mock of <see cref="MainWindowViewModel"/>
/// </summary>
public class MockMainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    private readonly MockViewModelProvider _vmProvider;

    /// <summary>
    /// A mock of <see cref="MainWindowViewModel"/>
    /// </summary>
    /// <param name="vmProvider">The ViewModelProvider used when creating <see cref="DisplayViewModel"/></param>
    public MockMainWindowViewModel(MockViewModelProvider vmProvider)
    {
        _vmProvider = vmProvider;
    }

    /// <summary>
    /// The backing field for <see cref="DisplayViewModel"/>
    /// </summary>
    private IDisplayViewModel? _displayViewModel;

    /// <summary>
    /// A reactive property that raises <see cref="ViewModelBase.PropertyChanged"/> event when changed
    /// </summary>
    public IDisplayViewModel? DisplayViewModel
    {
        get => _displayViewModel;
        private set => this.RaiseAndSetIfChanged(ref _displayViewModel, value);
    }

    /// <summary>
    /// Creates a new <see cref="MockDisplayViewModel"/> using <see cref="MockViewModelProvider"/>
    /// supplied in the <see cref="MockMainWindowViewModel(MockViewModelProvider)">constructor</see>,
    /// then sets <see cref="DisplayViewModel"/> to the new viewmodel instance
    /// </summary>
    /// <param name="id">The Id passed to <see cref="MockViewModelProvider.GetDisplayViewModel"/></param>
    /// <param name="contentProvider">
    /// The <see cref="IContentProvider"/> passed to <see cref="MockViewModelProvider.GetDisplayViewModel"/>
    /// </param>
    public void CreateDisplayViewModel(int id, IContentProvider contentProvider)
    {
        DisplayViewModel = _vmProvider.GetDisplayViewModel(id, contentProvider);
    }
}
