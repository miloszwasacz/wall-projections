using ReactiveUI;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.ViewModels.Display.Layouts;

public class DescriptionViewModel : ViewModelBase, ILayout
{
    private string _description;

    public DescriptionViewModel(string description)
    {
        Description = description;
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
