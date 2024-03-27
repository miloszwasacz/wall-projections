using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Test.Mocks.ViewModels.Editor;

public class MockImportViewModel : IImportViewModel
{
    public IDescriptionEditorViewModel DescriptionEditor { get; }

    public MockImportViewModel(IDescriptionEditorViewModel descriptionEditor)
    {
        DescriptionEditor = descriptionEditor;
    }

    public ImportWarningLevel IsImportSafe()
    {
        throw new NotImplementedException();
    }

    public bool ImportFromFile(string path)
    {
        throw new NotImplementedException();
    }
}
