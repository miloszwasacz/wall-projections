using System;
using ReactiveUI;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.ViewModels;

/// <inheritdoc cref="ISecondaryWindowViewModel" />
public class SecondaryWindowViewModel : ViewModelBase, ISecondaryWindowViewModel, IDisposable
{
    /// <summary>
    /// A <see cref="IViewModelProvider" /> used for creating child viewmodels.
    /// </summary>
    private readonly IViewModelProvider _vmProvider;

    /// <summary>
    /// The backing field for <see cref="Content" />.
    /// </summary>
    private ViewModelBase? _content;

    /// <inheritdoc />
    public ViewModelBase? Content
    {
        get => _content;
        private set
        {
            if (_content is IDisposable disposable)
                disposable.Dispose();

            this.RaiseAndSetIfChanged(ref _content, value);
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="SecondaryWindowViewModel" />.
    /// </summary>
    /// <param name="vmProvider">A <see cref="IViewModelProvider" /> used for creating child viewmodels.</param>
    public SecondaryWindowViewModel(IViewModelProvider vmProvider)
    {
        _vmProvider = vmProvider;
    }

    /// <inheritdoc />
    public void ShowHotspotDisplay(IConfig config)
    {
        Content = _vmProvider.GetHotspotDisplayViewModel(config);
    }

    /// <inheritdoc />
    public void ShowPositionEditor(IEditorViewModel editorViewModel)
    {
        Content = editorViewModel.PositionEditor;
    }

    /// <inheritdoc />
    public void ShowArUcoGrid()
    {
        Content = _vmProvider.GetArUcoGridViewModel();
    }

    public void Dispose()
    {
        if (_content is IDisposable disposable)
            disposable.Dispose();

        _content = null;
        GC.SuppressFinalize(this);
    }
}
