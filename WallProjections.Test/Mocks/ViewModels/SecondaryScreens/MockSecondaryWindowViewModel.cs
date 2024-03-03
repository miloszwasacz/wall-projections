﻿using ReactiveUI;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.Test.Mocks.ViewModels.SecondaryScreens;

public class MockSecondaryWindowViewModel : ViewModelBase, ISecondaryWindowViewModel
{
    /// <summary>
    /// The VM provider used for creating child viewmodels.
    /// </summary>
    private readonly MockViewModelProvider _vmProvider;

    /// <summary>
    /// The backing field for <see cref="Content" />.
    /// </summary>
    private ViewModelBase? _content;

    /// <inheritdoc />
    public ViewModelBase? Content
    {
        get => _content;
        private set => this.RaiseAndSetIfChanged(ref _content, value);
    }

    /// <summary>
    /// Creates a new instance of <see cref="MockSecondaryWindowViewModel" />.
    /// </summary>
    /// <param name="vmProvider">The VM provider used for creating child viewmodels.</param>
    public MockSecondaryWindowViewModel(MockViewModelProvider vmProvider)
    {
        _vmProvider = vmProvider;
    }

    /// <inheritdoc />
    public void ShowHotspotDisplay(IConfig config)
    {
        Content = _vmProvider.GetHotspotDisplayViewModel(config);
    }

    /// <inheritdoc />
    public void ShowPositionEditor()
    {
        Content = null;
    }

    /// <inheritdoc />
    public void ShowArUcoGrid()
    {
        Content = null;
    }
}
