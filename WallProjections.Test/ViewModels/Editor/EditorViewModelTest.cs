using System.Collections.Immutable;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using NUnit.Framework.Constraints;
using WallProjections.Helper;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.ViewModels.Editor;
using WallProjections.ViewModels.Editor;
using WallProjections.ViewModels.Interfaces.Editor;

// ReSharper disable AccessToStaticMemberViaDerivedType
using Is = WallProjections.Test.ViewModels.Editor.EditorViewModelTestExtensions.Is;
using Has = WallProjections.Test.ViewModels.Editor.EditorViewModelTestExtensions.Has;

namespace WallProjections.Test.ViewModels.Editor
{
    [TestFixture]
    public class EditorViewModelTest
    {
        private const int HotspotCount = 5;
        private static readonly string TestAssets = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets");
        private static string GetTestAsset(string file) => Path.Combine(TestAssets, file);
        private static readonly string DescriptionPath = GetTestAsset("test.txt");
        private static readonly string DescriptionText = File.ReadAllText(DescriptionPath);

        // ReSharper disable once InconsistentNaming
        private static readonly MockViewModelProvider VMProvider = new();

        /// <summary>
        /// Creates a test <see cref="IConfig" /> and a collection of <see cref="MockEditorHotspotViewModel" />s,
        /// which correspond to the hotspots in the config.
        /// </summary>
        private static (IConfig, ImmutableList<MockEditorHotspotViewModel>) CreateConfig()
        {
            var hotspots = new List<Hotspot>();
            var viewmodels = ImmutableList.CreateBuilder<MockEditorHotspotViewModel>();

            for (var i = 0; i < 2 * HotspotCount; i += 2)
            {
                var position = new Coord(i, i, i);
                var title = $"Test {i}";
                var images = new[] { "test_image.png", "test_image_2.jpg" };
                var imagePaths = ImmutableList.Create(images.Select(GetTestAsset).ToArray());
                var videos = new[] { "test_video.mp4" };
                var videosPaths = ImmutableList.Create(videos.Select(GetTestAsset).ToArray());

                var vm = new MockEditorHotspotViewModel(i, position, title, DescriptionText);
                foreach (var (name, path) in images.Zip(imagePaths))
                    vm.Images.Add(new MockThumbnailViewModel(path, name));
                foreach (var (name, path) in videos.Zip(videosPaths))
                    vm.Videos.Add(new MockThumbnailViewModel(path, name));

                hotspots.Add(new Hotspot(i, position, title, DescriptionPath, imagePaths, videosPaths));
                viewmodels.Add(vm);
            }

            return (new Config(hotspots), viewmodels.ToImmutable());
        }

        /// <summary>
        /// Uses <see cref="Window.StorageProvider" /> to get a file from the test assets.
        /// </summary>
        private static async Task<IStorageFile> GetFile(string fileName)
        {
            var window = new Window();
            var path = Path.Combine(TestAssets, fileName);
            var uri = new Uri($"file://{path}");

            return await window.StorageProvider.TryGetFileFromPathAsync(uri) ?? throw new FileNotFoundException(
                $"Could not find the file `{Path.GetFileName(uri.AbsolutePath)}`",
                uri.AbsolutePath
            );
        }

        [AvaloniaTest]
        public void EmptyConstructorTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            IEditorViewModel editorViewModel = new EditorViewModel(navigator, fileHandler, VMProvider);

            Assert.Multiple(() =>
            {
                Assert.That(editorViewModel.Hotspots, Is.Empty);
                Assert.That(editorViewModel.SelectedHotspot, Is.Null);
                Assert.That(
                    (editorViewModel.DescriptionEditor as MockDescriptionEditorViewModel)!.Hotspot,
                    Is.Null
                );
                Assert.That(editorViewModel.DescriptionEditor.Description, Is.Empty);
                Assert.That(editorViewModel.ImageEditor.Media, Is.Empty);
                Assert.That(editorViewModel.VideoEditor.Media, Is.Empty);
                Assert.That(editorViewModel, Is.Unsaved);
            });
        }

        [AvaloniaTest]
        public void FromConfigConstructorTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            var (config, expectedViewModels) = CreateConfig();
            IEditorViewModel editorViewModel = new EditorViewModel(config, navigator, fileHandler, VMProvider);

            Assert.That(editorViewModel.Hotspots, Is.Not.Empty);
            Assert.Multiple(() =>
            {
                Assert.That(editorViewModel.Hotspots, Has.EquivalentHotspots(expectedViewModels));
                Assert.That(editorViewModel.SelectedHotspot, Is.SameAs(editorViewModel.Hotspots[0]));
                Assert.That(editorViewModel.ImageEditor.Media, Is.EquivalentTo(editorViewModel.Hotspots[0].Images));
                Assert.That(editorViewModel.VideoEditor.Media, Is.EquivalentTo(editorViewModel.Hotspots[0].Videos));
                Assert.That(editorViewModel, Is.Saved);
            });
        }

        [AvaloniaTest]
        public void SetHotspotsTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            var (config, _) = CreateConfig();
            IEditorViewModel editorViewModel = new EditorViewModel(config, navigator, fileHandler, VMProvider);

            var hotspots = new[]
            {
                new MockEditorHotspotViewModel(
                    10,
                    new Coord(10, 10, 10),
                    "Test 10",
                    "Description 10"
                )
            };
            editorViewModel.Hotspots = new ObservableHotspotCollection<IEditorHotspotViewModel>(hotspots);

            Assert.Multiple(() =>
            {
                Assert.That(editorViewModel.Hotspots, Has.EquivalentHotspots(hotspots));
                Assert.That(editorViewModel.SelectedHotspot, Is.Null);
                Assert.That(editorViewModel.ImageEditor.Media, Is.Empty);
                Assert.That(editorViewModel.VideoEditor.Media, Is.Empty);
                Assert.That(editorViewModel, Is.Unsaved);
            });
        }

        [AvaloniaTest]
        public void SelectHotspotWhenSavedTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            var (config, expectedViewModels) = CreateConfig();
            IEditorViewModel editorViewModel = new EditorViewModel(config, navigator, fileHandler, VMProvider);

            var selected = expectedViewModels[1];
            editorViewModel.SelectedHotspot = selected;

            Assert.Multiple(() =>
            {
                Assert.That(editorViewModel.SelectedHotspot, Is.SameAs(selected));
                Assert.That(
                    (editorViewModel.DescriptionEditor as MockDescriptionEditorViewModel)!.Hotspot,
                    Is.SameAs(selected)
                );
                Assert.That(editorViewModel.ImageEditor.Media, Is.EquivalentTo(selected.Images));
                Assert.That(editorViewModel.VideoEditor.Media, Is.EquivalentTo(selected.Videos));
                Assert.That(editorViewModel, Is.Saved);
            });
        }

        [AvaloniaTest]
        public void SelectHotspotWhenUnsavedTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            var (config, expectedViewModels) = CreateConfig();
            IEditorViewModel editorViewModel = new EditorViewModel(config, navigator, fileHandler, VMProvider);

            // Asserts that modifying hotspot's properties doesn't change SelectedHotspot
            // and sets the editor to "unsaved".
            var selected = editorViewModel.SelectedHotspot!;
            selected.Title = "New Title";
            Assert.Multiple(() =>
            {
                Assert.That(editorViewModel.SelectedHotspot, Is.SameAs(selected));
                Assert.That(
                    (editorViewModel.DescriptionEditor as MockDescriptionEditorViewModel)!.Hotspot,
                    Is.SameAs(selected)
                );
                Assert.That(editorViewModel.ImageEditor.Media, Is.EquivalentTo(selected.Images));
                Assert.That(editorViewModel.VideoEditor.Media, Is.EquivalentTo(selected.Videos));
                Assert.That(editorViewModel, Is.Unsaved);
            });

            selected = expectedViewModels[2];
            editorViewModel.SelectedHotspot = selected;

            Assert.Multiple(() =>
            {
                Assert.That(editorViewModel.SelectedHotspot, Is.SameAs(selected));
                Assert.That(
                    (editorViewModel.DescriptionEditor as MockDescriptionEditorViewModel)!.Hotspot,
                    Is.SameAs(selected)
                );
                Assert.That(editorViewModel.ImageEditor.Media, Is.EquivalentTo(selected.Images));
                Assert.That(editorViewModel.VideoEditor.Media, Is.EquivalentTo(selected.Videos));
                Assert.That(editorViewModel, Is.Unsaved);
            });
        }

        [AvaloniaTest]
        public void AddHotspotTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            var (config, expectedViewModels) = CreateConfig();
            IEditorViewModel editorViewModel = new EditorViewModel(config, navigator, fileHandler, VMProvider);

            editorViewModel.AddHotspot();

            // Thanks to Dependency Injection we know that the new hotspot is a MockEditorHotspotViewModel
            var newHotspot = editorViewModel.Hotspots[^1] as MockEditorHotspotViewModel;
            expectedViewModels = expectedViewModels.Add(newHotspot!);

            Assert.Multiple(() =>
            {
                Assert.That(editorViewModel.Hotspots, Has.EquivalentHotspots(expectedViewModels));
                Assert.That(editorViewModel.SelectedHotspot, Is.SameAs(newHotspot));
                Assert.That(editorViewModel, Is.Unsaved);
            });

            // Asserts that the new hotspot has the lowest available ID.
            Assert.That(editorViewModel.SelectedHotspot!.Id, Is.EqualTo(1));
        }

        [AvaloniaTest]
        [TestCase(1, 1, TestName = "DeleteSelected")]
        [TestCase(1, 2, TestName = "DeleteUnselected")]
        public void DeleteHotspotTest(int selectedIndex, int deletedIndex)
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            var (config, expectedViewModels) = CreateConfig();
            IEditorViewModel editorViewModel = new EditorViewModel(config, navigator, fileHandler, VMProvider);

            editorViewModel.SelectedHotspot = editorViewModel.Hotspots[selectedIndex];
            var deleted = editorViewModel.Hotspots[deletedIndex];
            editorViewModel.DeleteHotspot(deleted);

            expectedViewModels = expectedViewModels.RemoveAt(deletedIndex);

            Assert.Multiple(() =>
            {
                Assert.That(editorViewModel.Hotspots, Has.EquivalentHotspots(expectedViewModels));
                Assert.That(editorViewModel.SelectedHotspot, Is.SameAs(editorViewModel.Hotspots[0]));
                Assert.That(editorViewModel, Is.Unsaved);
            });
        }

        [AvaloniaTest]
        [TestCase(MediaEditorType.Images, new[] { "test_image.png", "test_image_2.jpg" }, TestName = "AddImages")]
        [TestCase(MediaEditorType.Videos, new[] { "test_video.mp4" }, TestName = "AddVideos")]
        [TestCase((MediaEditorType)2, new[] { "" }, TestName = "AddInvalidType")]
        public async Task AddMediaTest(MediaEditorType expectedType, string[] expectedMediaPaths)
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            var (config, _) = CreateConfig();
            IEditorViewModel editorViewModel = new EditorViewModel(config, navigator, fileHandler, VMProvider);

            // Thanks to Dependency Injection we know that the selected hotspot is a MockEditorHotspotViewModel
            var selectedHotspot = editorViewModel.SelectedHotspot as MockEditorHotspotViewModel;
            var added = false;
            selectedHotspot!.MediaAdded += (_, args) =>
            {
                var mediaPaths = args.Files.Select(f => Path.GetFileName(f.Path.AbsolutePath));
                Assert.Multiple(() =>
                {
                    Assert.That(args.Type, Is.EqualTo(expectedType));
                    Assert.That(mediaPaths, Is.EquivalentTo(expectedMediaPaths));
                });
                added = true;
            };

            // If the expected type is invalid, the method should throw an exception
            if (expectedType > MediaEditorType.Videos)
            {
                Assert.That(
                    () => editorViewModel.AddMedia(expectedType, Array.Empty<IStorageFile>()),
                    Throws.InstanceOf<ArgumentOutOfRangeException>()
                );
                return;
            }

            var mediaToAdd = await Task.WhenAll(expectedMediaPaths.Select(GetFile));
            editorViewModel.AddMedia(expectedType, mediaToAdd);

            Assert.Multiple(() =>
            {
                Assert.That(added, Is.True);
                Assert.That(editorViewModel, Is.Unsaved);
            });
        }

        [AvaloniaTest]
        public void AddMediaNoHotspotSelectedTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            var (config, _) = CreateConfig();
            IEditorViewModel editorViewModel = new EditorViewModel(config, navigator, fileHandler, VMProvider);

            editorViewModel.SelectedHotspot = null;
            editorViewModel.AddMedia(MediaEditorType.Images, Array.Empty<IStorageFile>());

            Assert.Multiple(() =>
            {
                Assert.That(editorViewModel, Is.Saved);
                Assert.That(editorViewModel.ImageEditor.Media, Is.Empty);
                Assert.That(editorViewModel.VideoEditor.Media, Is.Empty);
            });
        }

        [AvaloniaTest]
        [TestCase(MediaEditorType.Images, new[] { "test_image.png", "test_image_2.jpg" }, TestName = "RemoveImages")]
        [TestCase(MediaEditorType.Videos, new[] { "test_video.mp4" }, TestName = "RemoveVideos")]
        [TestCase((MediaEditorType)2, new[] { "" }, TestName = "RemoveInvalidType")]
        public void RemoveMediaTest(MediaEditorType expectedType, string[] expectedMediaPaths)
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            var (config, _) = CreateConfig();
            IEditorViewModel editorViewModel = new EditorViewModel(config, navigator, fileHandler, VMProvider);

            // Thanks to Dependency Injection we know that the selected hotspot is a MockEditorHotspotViewModel
            var selectedHotspot = editorViewModel.SelectedHotspot as MockEditorHotspotViewModel;
            var removed = false;
            selectedHotspot!.MediaRemoved += (_, args) =>
            {
                var mediaPaths = args.Media.Select(t => Path.GetFileName(t.FilePath));
                Assert.Multiple(() =>
                {
                    Assert.That(args.Type, Is.EqualTo(expectedType));
                    Assert.That(mediaPaths, Is.EquivalentTo(expectedMediaPaths));
                });
                removed = true;
            };

            // If the expected type is invalid, the method should throw an exception
            if (expectedType > MediaEditorType.Videos)
            {
                Assert.That(
                    () => editorViewModel.RemoveMedia(expectedType, Array.Empty<IThumbnailViewModel>()),
                    Throws.InstanceOf<ArgumentOutOfRangeException>()
                );
                return;
            }

            // Select a media to remove
            var mediaEditor = expectedType switch
            {
                MediaEditorType.Images => editorViewModel.ImageEditor,
                MediaEditorType.Videos => editorViewModel.VideoEditor,
                _ => throw new ArgumentOutOfRangeException(nameof(expectedType), expectedType, null)
            };
            foreach (var media in expectedMediaPaths)
            {
                var selected = mediaEditor.Media.FirstOrDefault(m => m.FilePath.Contains(media));
                var index = mediaEditor.Media.IndexOf(selected!);
                mediaEditor.SelectedMedia.Select(index);
            }

            Assert.That(mediaEditor.SelectedMedia, Has.Count.EqualTo(expectedMediaPaths.Length));

            // The selected media and toRemove don't have to be the same instances (just in unit tests; normally they would be),
            // because we are using mocks and only care about the file paths.
            var toRemove = expectedMediaPaths.Select(p => new MockThumbnailViewModel(p, p));
            editorViewModel.RemoveMedia(expectedType, toRemove);

            Assert.Multiple(() =>
            {
                Assert.That(removed, Is.True);
                Assert.That(editorViewModel, Is.Unsaved);
                Assert.That(mediaEditor.SelectedMedia, Has.Count.Zero);
            });
        }

        [AvaloniaTest]
        public void RemoveMediaNoHotspotSelectedTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            var (config, _) = CreateConfig();
            IEditorViewModel editorViewModel = new EditorViewModel(config, navigator, fileHandler, VMProvider);

            editorViewModel.SelectedHotspot = null;
            editorViewModel.RemoveMedia(MediaEditorType.Images, Array.Empty<IThumbnailViewModel>());

            Assert.Multiple(() =>
            {
                Assert.That(editorViewModel, Is.Saved);
                Assert.That(editorViewModel.ImageEditor.Media, Is.Empty);
                Assert.That(editorViewModel.VideoEditor.Media, Is.Empty);
            });
        }

        [AvaloniaTest]
        [TestCase(true, TestName = "Successful")]
        [TestCase(false, TestName = "Unsuccessful")]
        public void SaveConfigTest(bool savingSuccessful)
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(savingSuccessful);
            var (config, _) = CreateConfig();
            IEditorViewModel editorViewModel = new EditorViewModel(config, navigator, fileHandler, VMProvider);

            editorViewModel.AddHotspot();
            Assert.That(editorViewModel, Is.Unsaved);

            var saved = editorViewModel.SaveConfig();

            Assert.Multiple(() =>
            {
                Assert.That(saved, Is.EqualTo(savingSuccessful));
                Assert.That(editorViewModel, savingSuccessful ? Is.Saved : Is.Unsaved);
                //TODO Improve Config comparison
                Assert.That(
                    fileHandler.Config.Hotspots.Select(h => h.Id),
                    Is.EquivalentTo(editorViewModel.Hotspots.Select(h => h.Id))
                );
            });
        }

        [AvaloniaTest]
        public void SaveConfigExceptionTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new IOException());
            var (config, _) = CreateConfig();
            IEditorViewModel editorViewModel = new EditorViewModel(config, navigator, fileHandler, VMProvider);

            editorViewModel.AddHotspot();
            editorViewModel.DeleteHotspot(editorViewModel.Hotspots[^1]);
            Assert.That(editorViewModel, Is.Unsaved);

            var saved = editorViewModel.SaveConfig();

            Assert.Multiple(() =>
            {
                Assert.That(saved, Is.False);
                Assert.That(editorViewModel, Is.Unsaved);
            });
        }

        [AvaloniaTest]
        public void SaveConfigNoHotspotsTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(true);
            var (config, _) = CreateConfig();
            IEditorViewModel editorViewModel = new EditorViewModel(config, navigator, fileHandler, VMProvider);

            editorViewModel.Hotspots.Clear();
            var saved = editorViewModel.SaveConfig();

            Assert.Multiple(() =>
            {
                Assert.That(saved, Is.True);
                Assert.That(editorViewModel, Is.Saved);
            });
        }

        [AvaloniaTest]
        [TestCase(true, TestName = "Successful")]
        [TestCase(false, TestName = "Unsuccessful")]
        public void ImportConfigTest(bool expectedSuccess)
        {
            const string filePath = "test.zip";
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(expectedSuccess);
            var (config, expectedViewModels) = CreateConfig();
            fileHandler.Config = config;
            IEditorViewModel editorViewModel = new EditorViewModel(navigator, fileHandler, VMProvider);

            editorViewModel.AddHotspot();
            editorViewModel.DeleteHotspot(editorViewModel.Hotspots[^1]);
            Assert.That(editorViewModel, Is.Unsaved);

            var imported = editorViewModel.ImportConfig(filePath);

            Assert.Multiple(() =>
            {
                Assert.That(imported, Is.EqualTo(expectedSuccess));
                Assert.That(editorViewModel, expectedSuccess ? Is.Saved : Is.Unsaved);
                Assert.That(fileHandler.LoadedZips, Is.EquivalentTo(new[] { filePath }));
                Assert.That(
                    editorViewModel.Hotspots,
                    expectedSuccess ? Has.EquivalentHotspots(expectedViewModels) : Is.Empty
                );
            });
            Assert.That(
                editorViewModel.SelectedHotspot,
                expectedSuccess ? Is.SameAs(editorViewModel.Hotspots[0]) : Is.Null
            );
        }

        [AvaloniaTest]
        public void ImportConfigExceptionTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new IOException());
            IEditorViewModel editorViewModel = new EditorViewModel(navigator, fileHandler, VMProvider);

            editorViewModel.AddHotspot();
            Assert.That(editorViewModel, Is.Unsaved);

            var imported = editorViewModel.ImportConfig("test.zip");

            Assert.Multiple(() =>
            {
                Assert.That(imported, Is.False);
                Assert.That(editorViewModel, Is.Unsaved);
                Assert.That(editorViewModel.Hotspots, Has.Exactly(1).Items);
            });
        }

        [AvaloniaTest]
        public void ImportConfigNoHotspotsTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(true)
            {
                Config = new Config(new List<Hotspot>())
            };
            IEditorViewModel editorViewModel = new EditorViewModel(navigator, fileHandler, VMProvider);

            editorViewModel.AddHotspot();
            Assert.That(editorViewModel, Is.Unsaved);

            var imported = editorViewModel.ImportConfig("test.zip");

            Assert.Multiple(() =>
            {
                Assert.That(imported, Is.True);
                Assert.That(editorViewModel, Is.Saved);
                Assert.That(editorViewModel.Hotspots, Is.Empty);
            });
        }

        [AvaloniaTest]
        [TestCase(true, TestName = "Successful")]
        [TestCase(false, TestName = "Unsuccessful")]
        public void ExportConfigTest(bool expectedSuccess)
        {
            const string exportPath = "test";
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(expectedSuccess);
            IEditorViewModel editorViewModel = new EditorViewModel(navigator, fileHandler, VMProvider);

            editorViewModel.AddHotspot();
            Assert.That(editorViewModel, Is.Unsaved);

            var exported = editorViewModel.ExportConfig(exportPath);

            Assert.Multiple(() =>
            {
                Assert.That(exported, Is.EqualTo(expectedSuccess));
                Assert.That(editorViewModel, Is.Unsaved);
                Assert.That(
                    fileHandler.ExportedZips,
                    Is.EquivalentTo(new[] { Path.Combine(exportPath, IEditorViewModel.ExportFileName) })
                );
            });
        }

        [AvaloniaTest]
        public void ExportConfigExceptionTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new IOException());
            IEditorViewModel editorViewModel = new EditorViewModel(navigator, fileHandler, VMProvider);

            editorViewModel.AddHotspot();
            Assert.That(editorViewModel, Is.Unsaved);

            var exported = editorViewModel.ExportConfig("test");

            Assert.Multiple(() =>
            {
                Assert.That(exported, Is.False);
                Assert.That(editorViewModel, Is.Unsaved);
            });
        }

        [AvaloniaTest]
        public void CloseEditorTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            navigator.OpenEditor();
            Assert.That(navigator.IsEditorOpen, Is.True);

            IEditorViewModel editorViewModel = new EditorViewModel(navigator, fileHandler, VMProvider);

            editorViewModel.CloseEditor();

            Assert.That(navigator.IsEditorOpen, Is.False);
        }
    }
}

#region NUnitExtensions

namespace WallProjections.Test.ViewModels.Editor.EditorViewModelTestExtensions
{
    /// <summary>
    /// Additional NUnit constraints for <see cref="IEditorViewModel" />s.
    /// </summary>
    internal static class NUnitExtensions
    {
        #region IsSaved

        /// <summary>
        /// Returns whether the given <see cref="IEditorViewModel" /> is
        /// <see cref="IEditorViewModel.IsSaved">saved</see>, and has
        /// the correct <see cref="IEditorViewModel.CloseButtonText" />,
        /// based on the value of <see cref="IEditorViewModel.IsSaved" />.
        /// </summary>
        /// <seealso cref="CorrectSavedCloseButtonText" />.
        /// <seealso cref="CorrectUnsavedCloseButtonText" />.
        public sealed class IsSavedConstraint : Constraint
        {
            // ReSharper disable once MemberCanBePrivate.Global
            /// <summary>
            /// The correct text for the close button when the editor is saved.
            /// </summary>
            public const string CorrectSavedCloseButtonText = "Close";

            // ReSharper disable once MemberCanBePrivate.Global
            /// <summary>
            /// The correct text for the close button when the editor is unsaved.
            /// </summary>
            public const string CorrectUnsavedCloseButtonText = "Discard";

            /// <summary>
            /// Whether <see cref="IEditorViewModel.IsSaved" /> should be <i>true</i>.
            /// </summary>
            private readonly bool _expectedIsSaved;

            public IsSavedConstraint(bool expectedIsSaved = true)
            {
                _expectedIsSaved = expectedIsSaved;
                Description = _expectedIsSaved
                    ? $"saved and close button has correct text (\"{CorrectSavedCloseButtonText}\")"
                    : $"unsaved and close button has correct text (\"{CorrectUnsavedCloseButtonText}\")";
            }

            public override ConstraintResult ApplyTo<TActual>(TActual actual)
            {
                if (actual is not IEditorViewModel editorViewModel)
                    return new ConstraintResult(this, actual, ConstraintStatus.Error);

                var expectedCloseButtonText = _expectedIsSaved
                    ? CorrectSavedCloseButtonText
                    : CorrectUnsavedCloseButtonText;

                var actualIsSaved = $"{nameof(editorViewModel.IsSaved)}: {editorViewModel.IsSaved}";
                var actualCloseButtonText =
                    $"{nameof(editorViewModel.CloseButtonText)}: `{editorViewModel.CloseButtonText}`";

                var matches =
                    editorViewModel.IsSaved == _expectedIsSaved &&
                    editorViewModel.CloseButtonText == expectedCloseButtonText;

                return new ConstraintResult(this, (actualIsSaved, actualCloseButtonText), matches);
            }
        }

        /// <summary>
        /// Appends a <see cref="IsSavedConstraint" /> to the given <see cref="ConstraintExpression" />.
        /// </summary>
        public static IsSavedConstraint IsSaved(this ConstraintExpression expression)
        {
            var constraint = new IsSavedConstraint();
            expression.Append(constraint);
            return constraint;
        }

        #endregion

        #region HasEquivalentHotspots

        /// <summary>
        /// Compares two collections of <see cref="IEditorHotspotViewModel" />s for equality by value.
        /// </summary>
        public static CollectionItemsEqualConstraint HasEquivalentHotspots(
            IEnumerable<IEditorHotspotViewModel> expectedCollection
        ) => NUnit.Framework.Is.EquivalentTo(expectedCollection)
            .Using<IEditorHotspotViewModel>((actual, expected) =>
            {
                var thumbnailComparer = new ThumbnailComparer();

                var id = actual.Id == expected.Id;
                var position = actual.Position == expected.Position;
                var title = actual.Title == expected.Title;
                var description = actual.Description == expected.Description;
                var images = actual.Images.SequenceEqual(expected.Images, thumbnailComparer);
                var videos = actual.Videos.SequenceEqual(expected.Videos, thumbnailComparer);

                return id && position && title && description && images && videos;
            });

        /// <summary>
        /// Appends a comparison of two collections of <see cref="IEditorHotspotViewModel" />s for equality by value
        /// to the given <see cref="ConstraintExpression" />.
        /// </summary>
        public static CollectionItemsEqualConstraint HasEquivalentHotspots(
            this ConstraintExpression expression,
            IEnumerable<IEditorHotspotViewModel> expectedCollection
        )
        {
            var constraint = HasEquivalentHotspots(expectedCollection);
            expression.Append(constraint);
            return constraint;
        }

        /// <summary>
        /// A comparer for <see cref="IThumbnailViewModel" />s that
        /// compares by <see cref="IThumbnailViewModel.FilePath" /> and <see cref="IThumbnailViewModel.Name" />.
        /// </summary>
        private class ThumbnailComparer : IEqualityComparer<IThumbnailViewModel>
        {
            public bool Equals(IThumbnailViewModel? x, IThumbnailViewModel? y)
            {
                if (x is null || y is null) return false;

                return x.FilePath == y.FilePath && x.Name == y.Name;
            }

            public int GetHashCode(IThumbnailViewModel obj)
            {
                return HashCode.Combine(obj.FilePath, obj.Name);
            }
        }

        #endregion
    }


    /// <inheritdoc />
    internal abstract class Is : NUnit.Framework.Is
    {
        /// <summary>
        /// Returns whether the given <see cref="IEditorViewModel" /> is
        /// <see cref="IEditorViewModel.IsSaved">saved</see>, and has
        /// the correct <see cref="IEditorViewModel.CloseButtonText" />,
        /// based on the value of <see cref="IEditorViewModel.IsSaved" />.
        /// </summary>
        /// <seealso cref="NUnitExtensions.IsSavedConstraint.CorrectSavedCloseButtonText" />.
        /// <seealso cref="Unsaved" />.
        public static NUnitExtensions.IsSavedConstraint Saved => new();

        /// <summary>
        /// Returns whether the given <see cref="IEditorViewModel" /> is
        /// <see cref="IEditorViewModel.IsSaved">unsaved</see>, and has
        /// the correct <see cref="IEditorViewModel.CloseButtonText" />,
        /// based on the value of <see cref="IEditorViewModel.IsSaved" />.
        /// </summary>
        /// <remarks>
        /// This is not the same as using <see cref="Is" />.<see cref="Is.Not" />.<see cref="Is.Saved" />,
        /// as it also checks the <see cref="IEditorViewModel.CloseButtonText" />
        /// (generally <i>Is.Not.Saved</i> should not be used).
        /// </remarks>
        /// <seealso cref="NUnitExtensions.IsSavedConstraint.CorrectUnsavedCloseButtonText" />.
        /// <seealso cref="Saved" />.
        public static NUnitExtensions.IsSavedConstraint Unsaved => new(false);
    }

    /// <inheritdoc />
    internal abstract class Has : NUnit.Framework.Has
    {
        /// <inheritdoc cref="NUnitExtensions.HasEquivalentHotspots(IEnumerable{IEditorHotspotViewModel})" />
        public static CollectionItemsEqualConstraint EquivalentHotspots(
            IEnumerable<IEditorHotspotViewModel> expectedCollection
        ) => NUnitExtensions.HasEquivalentHotspots(expectedCollection);
    }
}

#endregion
