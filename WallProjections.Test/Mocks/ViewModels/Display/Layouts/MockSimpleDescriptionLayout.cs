using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.Test.Mocks.ViewModels.Display.Layouts;

public class MockSimpleDescriptionLayout : Layout
{
    public string Title { get; }

    public string Description { get; }

    public MockSimpleDescriptionLayout(string title, string description) : base(null)
    {
        Title = title;
        Description = description;
    }
}
