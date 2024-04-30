using System.Collections.ObjectModel;
using Avalonia.Controls.Selection;
using WallProjections.Test.Mocks.ViewModels.Editor;
using WallProjections.ViewModels.Editor;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Test.ViewModels.Editor;

[TestFixture]
public class MediaEditorViewModelTest
{
    private const string Title = "Title";

    private static MockThumbnailViewModel CreateThumbnail(int id) => new($"path{id}", $"name{id}");

    [AvaloniaTest]
    public void ConstructorTest()
    {
        IMediaEditorViewModel mediaEditorViewModel = new MediaEditorViewModel(Title);

        Assert.Multiple(() =>
        {
            Assert.That(mediaEditorViewModel.Title, Is.EqualTo(Title));
            Assert.That(mediaEditorViewModel.IsEnabled, Is.False);
            Assert.That(mediaEditorViewModel.Media, Is.Empty);
            Assert.That(mediaEditorViewModel.SelectedMedia.SingleSelect, Is.False);
            Assert.That(mediaEditorViewModel.SelectedMedia.Count, Is.Zero);

            Assert.That(mediaEditorViewModel.CanRemoveMedia, Is.False);
            Assert.That(mediaEditorViewModel.AddMediaButtonLabel, Is.EqualTo($"Add {Title}"));
            Assert.That(mediaEditorViewModel.RemoveMediaButtonLabel, Is.EqualTo($"Remove {Title}"));

            Assert.That(IMediaEditorViewModel.ColumnCount, Is.EqualTo(2));
        });
    }

    [AvaloniaTest]
    public async Task SelectedMediaTest()
    {
        IMediaEditorViewModel mediaEditorViewModel = new MediaEditorViewModel(Title);

        var thumbnail1 = CreateThumbnail(1);
        var thumbnail2 = CreateThumbnail(2);

        mediaEditorViewModel.Media.Add(thumbnail1);
        mediaEditorViewModel.Media.Add(thumbnail2);

        var selectionChangedCount = 0;
        var expectedCanRemoveMedia = false;
        var expectedSelectedMedia = Array.Empty<MockThumbnailViewModel>();
        mediaEditorViewModel.SelectedMedia.SelectionChanged += (_, _) =>
        {
            Assert.Multiple(() =>
            {
                // ReSharper disable AccessToModifiedClosure
                Assert.That(mediaEditorViewModel.CanRemoveMedia, Is.EqualTo(expectedCanRemoveMedia));
                Assert.That(mediaEditorViewModel.SelectedMedia.SelectedItems, Is.EquivalentTo(expectedSelectedMedia));
                // ReSharper restore AccessToModifiedClosure
            });
            selectionChangedCount++;
        };

        expectedCanRemoveMedia = true;
        expectedSelectedMedia = new[] { thumbnail2 };

        mediaEditorViewModel.SelectedMedia.Select(1);
        await Task.Delay(100);
        Assert.That(selectionChangedCount, Is.EqualTo(1));


        expectedCanRemoveMedia = true;
        expectedSelectedMedia = new[] { thumbnail1, thumbnail2 };

        mediaEditorViewModel.SelectedMedia.Select(0);
        await Task.Delay(100);
        Assert.That(selectionChangedCount, Is.EqualTo(2));

        expectedCanRemoveMedia = true;
        expectedSelectedMedia = new[] { thumbnail1 };

        mediaEditorViewModel.SelectedMedia.Deselect(1);
        await Task.Delay(100);
        Assert.That(selectionChangedCount, Is.EqualTo(3));

        expectedCanRemoveMedia = false;
        expectedSelectedMedia = Array.Empty<MockThumbnailViewModel>();

        mediaEditorViewModel.SelectedMedia.Deselect(0);
        await Task.Delay(100);
        Assert.That(selectionChangedCount, Is.EqualTo(4));
    }

    [AvaloniaTest]
    public void SetMediaTest()
    {
        IMediaEditorViewModel mediaEditorViewModel = new MediaEditorViewModel(Title);

        var thumbnail0 = CreateThumbnail(0);
        mediaEditorViewModel.Media.Add(thumbnail0);
        mediaEditorViewModel.SelectedMedia.Select(0);

        Assert.Multiple(() =>
        {
            Assert.That(mediaEditorViewModel.Media, Is.EquivalentTo(new[] { thumbnail0 }));
            Assert.That(mediaEditorViewModel.SelectedMedia.Source, Is.EqualTo(mediaEditorViewModel.Media));
            Assert.That(mediaEditorViewModel.SelectedMedia.SelectedItems, Is.EquivalentTo(new[] { thumbnail0 }));
            Assert.That(mediaEditorViewModel.CanRemoveMedia, Is.True);
        });

        var thumbnail1 = CreateThumbnail(1);
        var thumbnail2 = CreateThumbnail(2);

        mediaEditorViewModel.Media = new ObservableCollection<IThumbnailViewModel> { thumbnail1, thumbnail2 };

        Assert.Multiple(() =>
        {
            Assert.That(mediaEditorViewModel.Media, Is.EquivalentTo(new[] { thumbnail1, thumbnail2 }));
            Assert.That(mediaEditorViewModel.SelectedMedia.Source, Is.EqualTo(mediaEditorViewModel.Media));
            Assert.That(mediaEditorViewModel.SelectedMedia.SelectedItems, Is.Empty);
            Assert.That(mediaEditorViewModel.CanRemoveMedia, Is.False);
        });
    }

    [AvaloniaTest]
    public void SetSelectedMediaTest()
    {
        var thumbnail0 = CreateThumbnail(0);
        var thumbnail1 = CreateThumbnail(1);
        var media = new ObservableCollection<IThumbnailViewModel> { thumbnail0, thumbnail1 };

        IMediaEditorViewModel mediaEditorViewModel = new MediaEditorViewModel(Title);
        mediaEditorViewModel.Media = media;

        mediaEditorViewModel.SelectedMedia = new SelectionModel<IThumbnailViewModel>
        {
            SingleSelect = false,
            Source = new[] { thumbnail1 }
        };

        Assert.Multiple(() =>
        {
            Assert.That(mediaEditorViewModel.SelectedMedia.Source, Is.SameAs(media));
            Assert.That(mediaEditorViewModel.SelectedMedia.SelectedItems, Is.Empty);
        });
    }

    [TestFixture]
    public class MediaEditorTypeExtensionsTest
    {
        // ReSharper disable once UnusedMember.Global
        [DatapointSource]
        public static IEnumerable<int> CountSource
        {
            get
            {
                for (var i = 0; i < 10; i++)
                    yield return i;

                for (var i = 10; i <= 100; i += 10)
                    yield return i + Random.Shared.Next(0, 10);
            }
        }

        [Test]
        public void NameTest()
        {
            Assert.Multiple(() =>
            {
                Assert.That(MediaEditorType.Images.Name(), Is.EqualTo("Images"));
                Assert.That(MediaEditorType.Videos.Name(), Is.EqualTo("Videos"));
                Assert.That(() => ((MediaEditorType)2).Name(), Throws.InstanceOf<ArgumentOutOfRangeException>());
            });
        }

        [Theory]
        public void NumberBasedLabelTest(int count)
        {
            Assume.That(count >= 0);

            Assert.Multiple(() =>
            {
                Assert.That(
                    MediaEditorType.Images.NumberBasedLabel(count),
                    Is.EqualTo(count == 1 ? "image" : "images")
                );
                Assert.That(
                    MediaEditorType.Videos.NumberBasedLabel(count),
                    Is.EqualTo(count == 1 ? "video" : "videos")
                );
                Assert.That(
                    () => ((MediaEditorType)2).NumberBasedLabel(count),
                    Throws.InstanceOf<ArgumentOutOfRangeException>()
                );
            });
        }
    }
}
