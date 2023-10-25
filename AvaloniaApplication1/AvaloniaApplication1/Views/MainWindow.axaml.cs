using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaApplication1.ViewModels;

namespace AvaloniaApplication1.Views;

public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnOpened(object sender, KeyEventArgs e)
    {
        var vm = DataContext as MainWindowViewModel;
        switch (e.Key)
        {
            case Key.Space:
                vm?.Play();
                break;
            case Key.Enter:
                vm?.OpenPython();
                break;
        }
    }
}