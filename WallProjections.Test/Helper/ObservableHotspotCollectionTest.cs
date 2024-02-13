using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using WallProjections.Helper;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Test.Helper;

[TestFixture]
public class ObservableHotspotCollectionTest
{
    /// <summary>
    /// Creates a list of sample items.
    /// </summary>
    /// <returns>A new of 5 <see cref="MockHotspotViewModel" />.</returns>
    private static List<MockHotspotViewModel> CreateTestItems()
    {
        var items = new List<MockHotspotViewModel>();
        for (var i = 0; i < 5; i++)
        {
            items.Add(new MockHotspotViewModel(
                i,
                new Coord(0, 0, 0),
                $"Title {i}",
                $"Description {i}"
            ));
        }

        return items;
    }

    private static MockHotspotViewModel CreateNewItem() => new(
        5,
        new Coord(0, 0, 0),
        "Title New",
        "Description New"
    );

    [Test]
    public void ConstructorEmptyTest()
    {
        var collection = new ObservableHotspotCollection<MockHotspotViewModel>();
        Assert.That(collection, Is.Empty);
    }

    [Test]
    public void ConstructorEnumerableTest()
    {
        var items = CreateTestItems();
        var collection = new ObservableHotspotCollection<MockHotspotViewModel>(
            (IEnumerable<MockHotspotViewModel>)items
        );
        Assert.That(collection, Is.EquivalentTo(items));
    }

    [Test]
    public void ConstructorListTest()
    {
        var items = CreateTestItems();
        var collection = new ObservableHotspotCollection<MockHotspotViewModel>(items);
        Assert.That(collection, Is.EquivalentTo(items));
    }

    [Test]
    [TestCase(0, 0, TestName = "None")]
    [TestCase(1, 1, TestName = "Once")]
    [TestCase(2, 2, TestName = "Twice")]
    public async Task ItemPropertyChangedTest(int id, int timesChanged)
    {
        var items = CreateTestItems();
        var collection = new ObservableHotspotCollection<MockHotspotViewModel>(items);
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

    [Test]
    public async Task ItemCollectionsChangedTest()
    {
        var items = CreateTestItems();
        var collection = new ObservableHotspotCollection<MockHotspotViewModel>(items);
        var changed = 0;
        collection.CollectionChanged += (_, _) =>
        {
            Assert.Fail("Collection changed event should not be triggered.");
            changed++;
        };

        items[0].Images.Add(new MockThumbnailViewModel());
        items[0].Videos.Add(new MockThumbnailViewModel());
        await Task.Delay(200);

        // Updating inner collections does not trigger a change event.
        Assert.That(changed, Is.EqualTo(0));
    }

    [Test]
    public async Task ClearItemsTest()
    {
        var items = CreateTestItems();
        var collection = new ObservableHotspotCollection<MockHotspotViewModel>(items);
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
        var collection = new ObservableHotspotCollection<MockHotspotViewModel>(items);
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
        var collection = new ObservableHotspotCollection<MockHotspotViewModel>(items);
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
        var collection = new ObservableHotspotCollection<MockHotspotViewModel>(items);
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

    /// <summary>
    /// A mock viewmodel used as items for testing <see cref="ObservableHotspotCollection{T}" />.
    /// </summary>
    public class MockHotspotViewModel : IEditorHotspotViewModel, INotifyPropertyChanged
    {
        private Coord _position;
        private string _title;
        private string _description;

        public int Id { get; }

        public Coord Position
        {
            get => _position;
            set
            {
                _position = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Position)));
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
            }
        }

        public ObservableCollection<IThumbnailViewModel> Images { get; }
        public ObservableCollection<IThumbnailViewModel> Videos { get; }

        public MockHotspotViewModel(
            int id,
            Coord position,
            string title,
            string description
        )
        {
            Id = id;
            _position = position;
            _title = title;
            _description = description;
            Images = new ObservableCollection<IThumbnailViewModel>();
            Videos = new ObservableCollection<IThumbnailViewModel>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Invokes <see cref="PropertyChanged"/> event with the given property name and <i>null</i> sender.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property passed to <see cref="PropertyChangedEventArgs(string)" />.
        /// </param>
        public void UntypedNotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        #region Unused

        public void AddMedia(MediaEditorType type, IEnumerable<IStorageFile> files)
        {
            throw new NotSupportedException();
        }

        public void RemoveMedia(MediaEditorType type, IEnumerable<IThumbnailViewModel> media)
        {
            throw new NotSupportedException();
        }

        public Hotspot ToHotspot()
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    private class MockThumbnailViewModel : IThumbnailViewModel
    {
        public int Row { get; set; } = 0;
        public int Column { get; set; } = 0;
        public string FilePath => "Path";
        public Bitmap Image => null!;
        public string Name => "Name";
    }
}
