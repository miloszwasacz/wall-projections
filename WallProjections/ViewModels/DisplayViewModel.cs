using WallProjections.Models;

namespace WallProjections.ViewModels;

public class DisplayViewModel : ViewModelBase
{
    public string Description { get; }

    public DisplayViewModel(string fileNumber)
    {
        var files = FileLocator.GetFiles(fileNumber);
        Description = files[0];
    }
}
