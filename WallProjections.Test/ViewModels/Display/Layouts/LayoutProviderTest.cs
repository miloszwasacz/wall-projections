using System.Collections.Immutable;
using System.Reflection;
using WallProjections.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.ViewModels.Display.Layouts;
using WallProjections.ViewModels.Display.Layouts;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.Test.ViewModels.Display.Layouts;

[TestFixture]
public class LayoutProviderTest
{
    /// <summary>
    /// Creates a new <see cref="Hotspot.Media" /> with ID 0 and no data.
    /// </summary>
    private static Hotspot.Media CreateHotspot() =>
        new(0, "Hotspot 0", "", ImmutableList<string>.Empty, ImmutableList<string>.Empty);

    [AvaloniaTest]
    public void ConstructorTest()
    {
        var availableLayouts = new[]
        {
            typeof(DescriptionViewModel.Factory),
            typeof(ImageWithDescriptionViewModel.Factory),
            typeof(VideoWithDescriptionViewModel.Factory)
        };

        var layoutProvider = new LayoutProvider();
        var layoutFactories = GetLayoutFactories(layoutProvider);
        Assert.That(
            layoutFactories,
            Is.EquivalentTo(availableLayouts).Using<LayoutFactory, Type>(
                (factory, type) => type.IsInstanceOfType(factory)
            )
        );
    }

    [Test]
    public void ConstructorWithFactoriesTest()
    {
        var availableLayouts = new[]
        {
            new MockLayoutFactory()
        };

        var layoutProvider = new LayoutProvider(availableLayouts);
        var layoutFactories = GetLayoutFactories(layoutProvider);
        Assert.That(layoutFactories, Is.EquivalentTo(availableLayouts));
    }

    [Test]
    public void GetLayoutTest()
    {
        var vmProvider = new MockViewModelProvider();
        var hotspot = CreateHotspot();
        var factory = new MockLayoutFactory();

        var layoutProvider = new LayoutProvider(new[] { factory });
        var layout = layoutProvider.GetLayout(vmProvider, hotspot);

        Assert.That(layout, Is.InstanceOf<MockGenericLayout>());
        var mockLayout = (MockGenericLayout)layout;
        Assert.That(mockLayout.Media, Is.EqualTo(hotspot));
    }

    [Test]
    public void GetLayoutNotFoundTest()
    {
        var vmProvider = new MockViewModelProvider();
        var hotspot = CreateHotspot();
        var factory = new MockLayoutFactory
        {
            IsCompatible = false
        };

        var layoutProvider = new LayoutProvider(new[] { factory });
        var layout = layoutProvider.GetLayout(vmProvider, hotspot);

        Assert.That(layout, Is.InstanceOf<DescriptionViewModel>());
        var description = (DescriptionViewModel)layout;
        Assert.Multiple(() =>
        {
            Assert.That(description.Title, Is.EqualTo("Error"));
            Assert.That(description.Description, Is.EqualTo(LayoutProvider.ErrorDescription));
        });
    }

    [Test]
    public void GetSimpleDescriptionLayoutTest()
    {
        const string title = "Title";
        const string description = "Description";

        var layoutProvider = new LayoutProvider();
        var layout = layoutProvider.GetSimpleDescriptionLayout(title, description);
        Assert.That(layout, Is.InstanceOf<DescriptionViewModel>());
        var descriptionLayout = (DescriptionViewModel)layout;
        Assert.Multiple(() =>
        {
            Assert.That(descriptionLayout.Title, Is.EqualTo(title));
            Assert.That(descriptionLayout.Description, Is.EqualTo(description));
        });
    }

    /// <summary>
    /// Gets the layout factories the provided <see cref="LayoutProvider" /> can create.
    /// </summary>
    private static IEnumerable<LayoutFactory> GetLayoutFactories(LayoutProvider layoutProvider)
    {
        const string fieldName = "_layoutFactories";
        var field = layoutProvider.GetType()
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return field?.GetValue(layoutProvider) as IEnumerable<LayoutFactory>
               ?? throw new MissingFieldException(nameof(LayoutProvider), fieldName);
    }
}
