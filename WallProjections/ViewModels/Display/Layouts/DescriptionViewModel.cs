using System;
using System.Threading.Tasks;
using ReactiveUI;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.ViewModels.Display.Layouts;

public class DescriptionViewModel : ViewModelBase, ILayout
{
    private string _description;
    private string _title;

    public DescriptionViewModel(string title, string description)
    {
        _description = description;
        _title = title;
    }

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    public void Dispose()
    {

    }
}
