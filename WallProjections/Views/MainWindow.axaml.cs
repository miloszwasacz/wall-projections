using System;
using Avalonia.Controls;
using Avalonia.Input;
using WallProjections.ViewModels;

namespace WallProjections.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel ViewModel => DataContext as MainWindowViewModel ??
                                             throw new NullReferenceException("Window does not have a ViewModel");
    
    public MainWindow()
    {
        InitializeComponent();
    }

    // ReSharper disable UnusedParameter.Local
    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        var key = e.Key switch
        {
            Key.D1 => "1",
            Key.D2 => "2",
            Key.D3 => "3",
            _ => null
        };

        if (key is not null)
            ViewModel.CreateDisplayViewModel(key);
    }
}
