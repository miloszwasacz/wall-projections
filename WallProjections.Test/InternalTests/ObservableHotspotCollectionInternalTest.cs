using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using WallProjections.Helper;
using WallProjections.Models;
using WallProjections.Test.Mocks.ViewModels.Editor;

namespace WallProjections.Test.InternalTests;

[TestFixture]
public class ObservableHotspotCollectionInternalTest
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

    /// <summary>
    /// Checks if the collection emits the <see cref="ObservableHotspotCollection{T}.CollectionChanged" /> event
    /// even if the <i>sender</i> is of the wrong type.
    /// </summary>
    [Test]
    public async Task WrongTypeItemPropertyChangedTest()
    {
        var items = CreateTestItems();
        var collection = new ObservableHotspotCollection<MockEditorHotspotViewModel>(items);
        var changed = false;
        collection.CollectionChanged += (_, e) =>
        {
            Assert.Multiple(() =>
            {
                Assert.That(collection.IsItemUpdating, Is.True);
                Assert.That(e.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            });
            changed = true;
        };

        items[2].UntypedNotifyPropertyChanged(nameof(MockEditorHotspotViewModel.Title));
        await Task.Delay(100);

        Assert.That(changed, Is.True);
    }

    /// <summary>
    /// Checks if the collection still emits the <see cref="ObservableHotspotCollection{T}.CollectionChanged" /> event
    /// even if the item is not found.
    /// </summary>
    [Test]
    public async Task NonexistentItemPropertyChangedTest()
    {
        var collection = new ObservableHotspotCollection<MockEditorHotspotViewModel>();
        var item = new MockEditorHotspotViewModel(
            0,
            new Coord(0, 0, 0),
            "Title",
            "Description"
        );
        var changed = false;
        collection.CollectionChanged += (_, e) =>
        {
            Assert.Multiple(() =>
            {
                Assert.That(collection.IsItemUpdating, Is.True);
                Assert.That(e.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            });
            changed = true;
        };

        var onItemPropertyChanged =
            collection.GetType().GetMethod("ItemPropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance)!;
        onItemPropertyChanged.Invoke(collection, new object[] { item, new PropertyChangedEventArgs("Nonexistent") });
        await Task.Delay(100);

        Assert.That(changed, Is.True);
    }
}
