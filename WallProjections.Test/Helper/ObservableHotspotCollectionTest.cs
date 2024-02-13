using Avalonia.Headless.NUnit;
using WallProjections.Helper;
using WallProjections.Models;
using WallProjections.Test.Mocks.ViewModels.Editor;

namespace WallProjections.Test.Helper;

[TestFixture]
public class ObservableHotspotCollectionTest
{
    /// <summary>
    /// Creates a list of sample items.
    /// </summary>
    /// <returns>A new of 5 <see cref="MockEditorHotspotViewModel" />.</returns>
    private static List<MockEditorHotspotViewModel> CreateTestItems()
    {
        var items = new List<MockEditorHotspotViewModel>();
        for (var i = 0; i < 5; i++)
        {
            items.Add(new MockEditorHotspotViewModel(
                i,
                new Coord(0, 0, 0),
                $"Title {i}",
                $"Description {i}"
            ));
        }

        return items;
    }

    private static MockEditorHotspotViewModel CreateNewItem() => new(
        5,
        new Coord(0, 0, 0),
        "Title New",
        "Description New"
    );

    private static MockThumbnailViewModel CreateThumbnailViewModel() => new(0, 0, "", "Name");

    [Test]
    public void ConstructorEmptyTest()
    {
        var collection = new ObservableHotspotCollection<MockEditorHotspotViewModel>();
        Assert.That(collection, Is.Empty);
    }

    [Test]
    public void ConstructorEnumerableTest()
    {
        var items = CreateTestItems();
        var collection = new ObservableHotspotCollection<MockEditorHotspotViewModel>(
            (IEnumerable<MockEditorHotspotViewModel>)items
        );
        Assert.That(collection, Is.EquivalentTo(items));
    }

    [Test]
    public void ConstructorListTest()
    {
        var items = CreateTestItems();
        var collection = new ObservableHotspotCollection<MockEditorHotspotViewModel>(items);
        Assert.That(collection, Is.EquivalentTo(items));
    }

    [Test]
    [TestCase(0, 0, TestName = "None")]
    [TestCase(1, 1, TestName = "Once")]
    [TestCase(2, 2, TestName = "Twice")]
    public async Task ItemPropertyChangedTest(int id, int timesChanged)
    {
        var items = CreateTestItems();
        var collection = new ObservableHotspotCollection<MockEditorHotspotViewModel>(items);
        var changed = 0;
        collection.CollectionChanged += (_, _) =>
        {
            Assert.That(collection.IsItemUpdating, Is.True);
            changed++;
        };

        for (var i = 0; i < timesChanged; i++)
            items[id].Title = $"Changed {i}";
        await Task.Delay(timesChanged * 100);

        Assert.That(changed, Is.EqualTo(timesChanged));
    }

    [AvaloniaTest]
    public async Task ItemCollectionsChangedTest()
    {
        var items = CreateTestItems();
        var collection = new ObservableHotspotCollection<MockEditorHotspotViewModel>(items);
        var changed = 0;
        collection.CollectionChanged += (_, _) =>
        {
            Assert.Fail("Collection changed event should not be triggered.");
            changed++;
        };

        items[0].Images.Add(CreateThumbnailViewModel());
        items[0].Videos.Add(CreateThumbnailViewModel());
        await Task.Delay(200);

        // Updating inner collections does not trigger a change event.
        Assert.That(changed, Is.EqualTo(0));
    }

    [Test]
    public async Task ClearItemsTest()
    {
        var items = CreateTestItems();
        var collection = new ObservableHotspotCollection<MockEditorHotspotViewModel>(items);
        var changed = false;
        collection.CollectionChanged += (_, _) =>
        {
            Assert.That(collection.IsItemUpdating, Is.False);
            changed = true;
        };

        collection.Clear();
        await Task.Delay(100);

        Assert.Multiple(() =>
        {
            Assert.That(changed, Is.True);
            Assert.That(collection, Is.Empty);
        });
    }

    [Test]
    public async Task InsertItemTest()
    {
        var items = CreateTestItems();
        var collection = new ObservableHotspotCollection<MockEditorHotspotViewModel>(items);
        var changed = false;
        collection.CollectionChanged += (_, _) =>
        {
            Assert.That(collection.IsItemUpdating, Is.False);
            changed = true;
        };

        var newItem = CreateNewItem();
        collection.Insert(0, newItem);
        await Task.Delay(100);

        Assert.Multiple(() =>
        {
            Assert.That(changed, Is.True);
            Assert.That(collection, Has.Count.EqualTo(items.Count + 1));
            Assert.That(collection[0], Is.EqualTo(newItem));
        });
        for (var i = 0; i < items.Count; i++)
            Assert.That(collection[i + 1], Is.EqualTo(items[i]));
    }

    [Test]
    public async Task RemoveItemTest()
    {
        var items = CreateTestItems();
        var collection = new ObservableHotspotCollection<MockEditorHotspotViewModel>(items);
        var changed = false;
        collection.CollectionChanged += (_, _) =>
        {
            Assert.That(collection.IsItemUpdating, Is.False);
            changed = true;
        };

        collection.RemoveAt(0);
        await Task.Delay(100);

        Assert.Multiple(() =>
        {
            Assert.That(changed, Is.True);
            Assert.That(collection, Has.Count.EqualTo(items.Count - 1));
        });

        for (var i = 1; i < items.Count; i++)
            Assert.That(collection[i - 1], Is.EqualTo(items[i]));
    }

    [Test]
    public async Task SetItemTest()
    {
        var items = CreateTestItems();
        var collection = new ObservableHotspotCollection<MockEditorHotspotViewModel>(items);
        var changed = false;
        collection.CollectionChanged += (_, _) =>
        {
            Assert.That(collection.IsItemUpdating, Is.False);
            changed = true;
        };

        var newItem = CreateNewItem();
        collection[0] = newItem;
        await Task.Delay(100);

        Assert.Multiple(() =>
        {
            Assert.That(changed, Is.True);
            Assert.That(collection, Has.Count.EqualTo(items.Count));
            Assert.That(collection[0], Is.EqualTo(newItem));
        });
        for (var i = 1; i < items.Count; i++)
            Assert.That(collection[i], Is.EqualTo(items[i]));
    }
}
