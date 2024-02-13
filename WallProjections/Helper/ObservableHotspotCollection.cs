using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Helper;

/// <summary>
/// <inheritdoc cref="ObservableCollection{T}" />
/// Additionally, listens for <see cref="INotifyPropertyChanged.PropertyChanged" />
/// of each item and refreshes the collection accordingly.
/// </summary>
/// <typeparam name="T">
/// The type of elements in the collection.
/// Must implement <see cref="IEditorHotspotViewModel" /> and <see cref="INotifyPropertyChanged" />.
/// </typeparam>
public class ObservableHotspotCollection<T> : ObservableCollection<T>
    where T : IEditorHotspotViewModel, INotifyPropertyChanged
{
    /// <summary>
    /// A mutex to enforce that only one item can be updated at a time.
    /// </summary>
    private readonly Mutex _mutex = new();

    /// <summary>
    /// Whether the collection is currently updating because of a property change in an item.
    /// </summary>
    public bool IsItemUpdating { get; private set; }

    /// <inheritdoc cref="ObservableCollection{T}()"/>
    /// <seealso cref="ObservableCollection{T}()"/>
    public ObservableHotspotCollection()
    {
    }

    /// <summary>
    /// <inheritdoc cref="ObservableCollection{T}(IEnumerable{T})"/>
    /// Then, attaches handlers to <see cref="INotifyPropertyChanged.PropertyChanged"/> of each item.
    /// </summary>
    /// <inheritdoc cref="ObservableCollection{T}(IEnumerable{T})"/>
    /// <seealso cref="ObservableCollection{T}(IEnumerable{T})"/>
    public ObservableHotspotCollection(IEnumerable<T> collection) : base(collection)
    {
        ListenForChanges();
    }

    /// <summary>
    /// <inheritdoc cref="ObservableCollection{T}(List{T})"/>
    /// Then, attaches handlers to <see cref="INotifyPropertyChanged.PropertyChanged"/> of each item.
    /// </summary>
    /// <inheritdoc cref="ObservableCollection{T}(List{T})"/>
    /// <seealso cref="ObservableCollection{T}(List{T})"/>
    public ObservableHotspotCollection(List<T> list) : base(list)
    {
        ListenForChanges();
    }

    /// <summary>
    /// Attaches handlers to <see cref="INotifyPropertyChanged.PropertyChanged"/> of each item
    /// in <see cref="ObservableHotspotCollection{T}.Items"/>.
    /// </summary>
    private void ListenForChanges()
    {
        foreach (var item in Items)
            item.PropertyChanged += ItemPropertyChanged;
    }

    /// <inheritdoc />
    protected override void ClearItems()
    {
        foreach (var item in Items)
            item.PropertyChanged -= ItemPropertyChanged;
        base.ClearItems();
    }

    /// <inheritdoc />
    protected override void InsertItem(int index, T item)
    {
        item.PropertyChanged += ItemPropertyChanged;
        base.InsertItem(index, item);
    }

    /// <inheritdoc />
    protected override void RemoveItem(int index)
    {
        this[index].PropertyChanged -= ItemPropertyChanged;
        base.RemoveItem(index);
    }

    /// <inheritdoc />
    protected override void SetItem(int index, T item)
    {
        this[index].PropertyChanged -= ItemPropertyChanged;
        item.PropertyChanged += ItemPropertyChanged;
        base.SetItem(index, item);
    }

    /// <summary>
    /// Notifies the collection to update itself. This is the callback
    /// attached to <see cref="INotifyPropertyChanged.PropertyChanged"/> of each item.
    /// </summary>
    /// <param name="sender">The item that has changed.</param>
    /// <param name="e">PropertyChanged arguments (unused).</param>
    /// <seealso cref="IsItemUpdating" />
    private void ItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        StartUpdate();
        if (sender is T item)
        {
            var index = IndexOf(item);
            if (index != -1)
            {
                Move(index, index);
                FinishUpdate();
                return;
            }
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        FinishUpdate();
        return;

        void StartUpdate()
        {
            _mutex.WaitOne();
            IsItemUpdating = true;
        }

        void FinishUpdate()
        {
            IsItemUpdating = false;
            _mutex.ReleaseMutex();
        }
    }
}
