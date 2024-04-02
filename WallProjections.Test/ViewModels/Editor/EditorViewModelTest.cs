using System.Collections.Immutable;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using NUnit.Framework.Constraints;
using WallProjections.Helper;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks.Helper;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.ViewModels.Editor;
using WallProjections.ViewModels.Editor;
using WallProjections.ViewModels.Interfaces.Editor;

// ReSharper disable AccessToStaticMemberViaDerivedType
using Has = WallProjections.Test.ViewModels.Editor.EditorViewModelTestExtensions.Has;

namespace WallProjections.Test.ViewModels.Editor
{
    //TODO Add assertions for the homography matrix in the Config & VM
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

            return (new Config(MockPythonProxy.CalibrationResult, hotspots), viewmodels.ToImmutable());
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
                $"Could not find the file `{Path.GetFileName(uri.LocalPath)}`",
                uri.LocalPath
            );
        }

        [AvaloniaTest]
        public void EmptyConstructorTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            var pythonHandler = new MockPythonHandler();
            IEditorViewModel editorViewModel = new EditorViewModel(navigator, fileHandler, pythonHandler, VMProvider);

            Assert.Multiple(() =>
            {
                Assert.That(editorViewModel.Hotspots, Is.Empty);
                Assert.That(editorViewModel.SelectedHotspot, Is.Null);
                Assert.That(
                    (editorViewModel.DescriptionEditor as MockDescriptionEditorViewModel)!.Hotspot,
                    Is.Null
                );
                AssertPositionEditor(editorViewModel, new Coord(0, 0, 0), Enumerable.Empty<Coord>());
                Assert.That(editorViewModel.DescriptionEditor.Description, Is.Empty);
                Assert.That(editorViewModel.ImageEditor.Media, Is.Empty);
                Assert.That(editorViewModel.VideoEditor.Media, Is.Empty);
                Assert.That(editorViewModel.IsSaved);
                Assert.That(editorViewModel.IsImportSafe, Is.True);
                Assert.That(editorViewModel.CanExport, Is.False);
            });
        }

        [AvaloniaTest]
        public void FromConfigConstructorTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            var pythonHandler = new MockPythonHandler();
            var (config, expectedViewModels) = CreateConfig();
            var expectedSelectedCoord = config.Hotspots[0].Position;
            var expectedUnselectedCoord = config.Hotspots.Skip(1).Select(h => h.Position);
            IEditorViewModel editorViewModel =
                new EditorViewModel(config, navigator, fileHandler, pythonHandler, VMProvider);

            var descriptionEditor = (MockDescriptionEditorViewModel)editorViewModel.DescriptionEditor;
            Assert.That(editorViewModel.Hotspots, Is.Not.Empty);
            Assert.Multiple(() =>
            {
                Assert.That(editorViewModel.Hotspots, Has.EquivalentHotspots(expectedViewModels));
                Assert.That(editorViewModel.SelectedHotspot, Is.SameAs(editorViewModel.Hotspots[0]));
                AssertPositionEditor(editorViewModel, expectedSelectedCoord, expectedUnselectedCoord);
                Assert.That(descriptionEditor.Hotspot, Is.SameAs(editorViewModel.Hotspots[0]));
                Assert.That(editorViewModel.ImageEditor.Media, Is.EquivalentTo(editorViewModel.Hotspots[0].Images));
                Assert.That(editorViewModel.VideoEditor.Media, Is.EquivalentTo(editorViewModel.Hotspots[0].Videos));
                Assert.That(editorViewModel.IsSaved);
                Assert.That(editorViewModel.IsImportSafe, Is.False);
                Assert.That(editorViewModel.CanExport, Is.True);
            });
        }

        [AvaloniaTest]
        public void SetHotspotsTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            var pythonHandler = new MockPythonHandler();
            var (config, _) = CreateConfig();
            IEditorViewModel editorViewModel =
                new EditorViewModel(config, navigator, fileHandler, pythonHandler, VMProvider);

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
                Assert.That(editorViewModel.IsSaved, Is.False);
                Assert.That(editorViewModel.CanExport, Is.False);
            });
        }

        [AvaloniaTest]
        public void SelectHotspotWhenSavedTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            var pythonHandler = new MockPythonHandler();
            var (config, expectedViewModels) = CreateConfig();
            IEditorViewModel editorViewModel =
                new EditorViewModel(config, navigator, fileHandler, pythonHandler, VMProvider);

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
                Assert.That(editorViewModel.IsSaved);
                Assert.That(editorViewModel.CanExport, Is.True);
            });
        }

        [AvaloniaTest]
        public void SelectHotspotWhenUnsavedTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            var pythonHandler = new MockPythonHandler();
            var (config, expectedViewModels) = CreateConfig();
            IEditorViewModel editorViewModel =
                new EditorViewModel(config, navigator, fileHandler, pythonHandler, VMProvider);

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
                Assert.That(editorViewModel.IsSaved, Is.False);
                Assert.That(editorViewModel.CanExport, Is.False);
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
                Assert.That(editorViewModel.IsSaved, Is.False);
                Assert.That(editorViewModel.CanExport, Is.False);
            });
        }

        [AvaloniaTest]
        public void AddHotspotTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            var pythonHandler = new MockPythonHandler();
            var (config, expectedViewModels) = CreateConfig();
            IEditorViewModel editorViewModel =
                new EditorViewModel(config, navigator, fileHandler, pythonHandler, VMProvider);

            editorViewModel.AddHotspot();

            // Thanks to Dependency Injection we know that the new hotspot is a MockEditorHotspotViewModel
            var newHotspot = editorViewModel.Hotspots[^1] as MockEditorHotspotViewModel;
            expectedViewModels = expectedViewModels.Add(newHotspot!);

            Assert.Multiple(() =>
            {
                Assert.That(editorViewModel.Hotspots, Has.EquivalentHotspots(expectedViewModels));
                Assert.That(editorViewModel.SelectedHotspot, Is.SameAs(newHotspot));
                Assert.That(editorViewModel.IsSaved, Is.False);
                Assert.That(editorViewModel.CanExport, Is.False);
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
            var pythonHandler = new MockPythonHandler();
            var (config, expectedViewModels) = CreateConfig();
            IEditorViewModel editorViewModel =
                new EditorViewModel(config, navigator, fileHandler, pythonHandler, VMProvider);

            editorViewModel.SelectedHotspot = editorViewModel.Hotspots[selectedIndex];
            var deleted = editorViewModel.Hotspots[deletedIndex];
            editorViewModel.DeleteHotspot(deleted);

            expectedViewModels = expectedViewModels.RemoveAt(deletedIndex);

            Assert.Multiple(() =>
            {
                Assert.That(editorViewModel.Hotspots, Has.EquivalentHotspots(expectedViewModels));
                Assert.That(editorViewModel.SelectedHotspot, Is.SameAs(editorViewModel.Hotspots[0]));
                Assert.That(editorViewModel.IsSaved, Is.False);
                Assert.That(editorViewModel.CanExport, Is.False);
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
            var pythonHandler = new MockPythonHandler();
            var (config, _) = CreateConfig();
            IEditorViewModel editorViewModel =
                new EditorViewModel(config, navigator, fileHandler, pythonHandler, VMProvider);

            // Thanks to Dependency Injection we know that the selected hotspot is a MockEditorHotspotViewModel
            var selectedHotspot = editorViewModel.SelectedHotspot as MockEditorHotspotViewModel;
            var added = false;
            selectedHotspot!.MediaAdded += (_, args) =>
            {
                var mediaPaths = args.Files.Select(f => Path.GetFileName(f.Path.LocalPath));
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
                Assert.That(editorViewModel.IsSaved, Is.False);
                Assert.That(editorViewModel.CanExport, Is.False);
            });
        }

        [AvaloniaTest]
        public void AddMediaNoHotspotSelectedTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            var pythonHandler = new MockPythonHandler();
            var (config, _) = CreateConfig();
            IEditorViewModel editorViewModel =
                new EditorViewModel(config, navigator, fileHandler, pythonHandler, VMProvider);

            editorViewModel.SelectedHotspot = null;
            editorViewModel.AddMedia(MediaEditorType.Images, Array.Empty<IStorageFile>());

            Assert.Multiple(() =>
            {
                Assert.That(editorViewModel.IsSaved);
                Assert.That(editorViewModel.CanExport, Is.True);
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
            var pythonHandler = new MockPythonHandler();
            var (config, _) = CreateConfig();
            IEditorViewModel editorViewModel =
                new EditorViewModel(config, navigator, fileHandler, pythonHandler, VMProvider);

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
                Assert.That(editorViewModel.IsSaved, Is.False);
                Assert.That(editorViewModel.CanExport, Is.False);
                Assert.That(mediaEditor.SelectedMedia, Has.Count.Zero);
            });
        }

        [AvaloniaTest]
        public void RemoveMediaNoHotspotSelectedTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            var pythonHandler = new MockPythonHandler();
            var (config, _) = CreateConfig();
            IEditorViewModel editorViewModel =
                new EditorViewModel(config, navigator, fileHandler, pythonHandler, VMProvider);

            editorViewModel.SelectedHotspot = null;
            editorViewModel.RemoveMedia(MediaEditorType.Images, Array.Empty<IThumbnailViewModel>());

            Assert.Multiple(() =>
            {
                Assert.That(editorViewModel.IsSaved);
                Assert.That(editorViewModel.CanExport, Is.True);
                Assert.That(editorViewModel.ImageEditor.Media, Is.Empty);
                Assert.That(editorViewModel.VideoEditor.Media, Is.Empty);
            });
        }

        [AvaloniaTest]
        public async Task WithActionLockTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(true);
            var pythonHandler = new MockPythonHandler();
            var (config, _) = CreateConfig();
            IEditorViewModel editorViewModel =
                new EditorViewModel(config, navigator, fileHandler, pythonHandler, VMProvider);

            var result1 = false;
            var result2 = false;

            // Start the first task
            var task = editorViewModel.WithActionLock(async () =>
            {
                await Task.Delay(500);
                result1 = true;
            });
            Assert.That(editorViewModel.AreActionsDisabled, Is.True);

            // Start the second task (should be ignored)
            var success2 = await editorViewModel.WithActionLock(() =>
            {
                result2 = true;
                return Task.CompletedTask;
            });
            var success1 = await task;
            Assert.That(editorViewModel.AreActionsDisabled, Is.False);

            Assert.Multiple(() =>
            {
                Assert.That(result1, Is.True);
                Assert.That(result2, Is.False);
                Assert.That(success1, Is.True);
                Assert.That(success2, Is.False);
            });
        }

        [AvaloniaTest]
        public async Task WithActionLockExceptionTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(true);
            var pythonHandler = new MockPythonHandler();
            var (config, _) = CreateConfig();
            IEditorViewModel editorViewModel =
                new EditorViewModel(config, navigator, fileHandler, pythonHandler, VMProvider);

            var task = editorViewModel.WithActionLock(async () =>
            {
                await Task.Delay(500);
                throw new Exception();
            });
            Assert.That(editorViewModel.AreActionsDisabled, Is.True);

            try
            {
                await task;
                Assert.Fail("The task should have thrown an exception");
            }
            catch (Exception)
            {
                // Assert that the editor releases the lock
                Assert.That(editorViewModel.AreActionsDisabled, Is.False);
            }

            // Assert that the lock can be acquired again
            var lock2 = editorViewModel.TryAcquireActionLock();
            Assert.Multiple(() =>
            {
                Assert.That(lock2, Is.True);
                Assert.That(editorViewModel.AreActionsDisabled, Is.True);
            });
        }

        [AvaloniaTest]
        public void ManualActionLockTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(true);
            var pythonHandler = new MockPythonHandler();
            var (config, _) = CreateConfig();
            IEditorViewModel editorViewModel =
                new EditorViewModel(config, navigator, fileHandler, pythonHandler, VMProvider);

            // Acquire the lock
            var lock1 = editorViewModel.TryAcquireActionLock();
            Assert.Multiple(() =>
            {
                Assert.That(lock1, Is.True);
                Assert.That(editorViewModel.AreActionsDisabled, Is.True);
            });

            // Try to acquire the lock again
            var lock2 = editorViewModel.TryAcquireActionLock();
            Assert.Multiple(() =>
            {
                Assert.That(lock2, Is.False);
                Assert.That(editorViewModel.AreActionsDisabled, Is.True);
            });

            // Release the lock
            editorViewModel.ReleaseActionLock();
            Assert.That(editorViewModel.AreActionsDisabled, Is.False);

            // Try to acquire the lock again
            var lock3 = editorViewModel.TryAcquireActionLock();
            Assert.Multiple(() =>
            {
                Assert.That(lock3, Is.True);
                Assert.That(editorViewModel.AreActionsDisabled, Is.True);
            });
            editorViewModel.ReleaseActionLock();
        }

        [AvaloniaTest]
        [TestCase(true, TestName = "Successful")]
        [TestCase(false, TestName = "Unsuccessful")]
        public async Task SaveConfigTest(bool savingSuccessful)
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(savingSuccessful)
            {
                Delay = 500
            };
            var pythonHandler = new MockPythonHandler();
            var (config, _) = CreateConfig();
            IEditorViewModel editorViewModel =
                new EditorViewModel(config, navigator, fileHandler, pythonHandler, VMProvider);

            editorViewModel.AddHotspot();
            Assert.Multiple(() =>
            {
                Assert.That(editorViewModel.IsSaved, Is.False);
                Assert.That(editorViewModel.CanExport, Is.False);
            });

            var task = editorViewModel.SaveConfig();
            Assert.That(editorViewModel.IsSaving, Is.True);

            var saved = await task;
            Assert.That(editorViewModel.IsSaving, Is.False);

            Assert.Multiple(() =>
            {
                Assert.That(saved, Is.EqualTo(savingSuccessful));
                Assert.That(editorViewModel.IsSaved, savingSuccessful ? Is.True : Is.False);
                Assert.That(editorViewModel.CanExport, Is.EqualTo(savingSuccessful));
                //TODO Improve Config comparison
                Assert.That(
                    fileHandler.Config.Hotspots.Select(h => h.Id),
                    Is.EquivalentTo(editorViewModel.Hotspots.Select(h => h.Id))
                );
            });
        }

        [AvaloniaTest]
        public async Task SaveConfigExceptionTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new IOException());
            var pythonHandler = new MockPythonHandler();
            var (config, _) = CreateConfig();
            IEditorViewModel editorViewModel =
                new EditorViewModel(config, navigator, fileHandler, pythonHandler, VMProvider);

            editorViewModel.AddHotspot();
            editorViewModel.DeleteHotspot(editorViewModel.Hotspots[^1]);
            Assert.That(editorViewModel.IsSaved, Is.False);

            var saved = await editorViewModel.SaveConfig();

            Assert.Multiple(() =>
            {
                Assert.That(saved, Is.False);
                Assert.That(editorViewModel.IsSaved, Is.False);
                Assert.That(editorViewModel.CanExport, Is.False);
            });
        }

        [AvaloniaTest]
        public async Task SaveConfigNoHotspotsTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(true);
            var pythonHandler = new MockPythonHandler();
            var (config, _) = CreateConfig();
            IEditorViewModel editorViewModel =
                new EditorViewModel(config, navigator, fileHandler, pythonHandler, VMProvider);

            editorViewModel.Hotspots.Clear();
            var saved = await editorViewModel.SaveConfig();

            Assert.Multiple(() =>
            {
                Assert.That(saved, Is.True);
                Assert.That(editorViewModel.IsSaved);
            });
        }

        [AvaloniaTest]
        [TestCase(true, TestName = "Successful")]
        [TestCase(false, TestName = "Unsuccessful")]
        public async Task ImportConfigTest(bool expectedSuccess)
        {
            const string filePath = "test.zip";
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(expectedSuccess)
            {
                Delay = 500
            };
            var pythonHandler = new MockPythonHandler();
            var (config, expectedViewModels) = CreateConfig();
            fileHandler.Config = config;
            IEditorViewModel editorViewModel = new EditorViewModel(navigator, fileHandler, pythonHandler, VMProvider);

            editorViewModel.AddHotspot();
            editorViewModel.DeleteHotspot(editorViewModel.Hotspots[^1]);
            Assert.Multiple(() =>
            {
                Assert.That(editorViewModel.IsSaved, Is.False);
                Assert.That(editorViewModel.CanExport, Is.False);
            });

            var task = editorViewModel.ImportConfig(filePath);
            Assert.That(editorViewModel.IsImporting, Is.True);

            var imported = await task;
            Assert.That(editorViewModel.IsImporting, Is.False);

            Assert.Multiple(() =>
            {
                Assert.That(imported, Is.EqualTo(expectedSuccess));
                Assert.That(editorViewModel.IsSaved, expectedSuccess ? Is.True : Is.False);
                Assert.That(editorViewModel.CanExport, Is.EqualTo(expectedSuccess));
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
        public async Task ImportConfigExceptionTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new IOException());
            var pythonHandler = new MockPythonHandler();
            IEditorViewModel editorViewModel = new EditorViewModel(navigator, fileHandler, pythonHandler, VMProvider);

            editorViewModel.AddHotspot();
            Assert.That(editorViewModel.IsSaved, Is.False);

            var imported = await editorViewModel.ImportConfig("test.zip");

            Assert.Multiple(() =>
            {
                Assert.That(imported, Is.False);
                Assert.That(editorViewModel.IsSaved, Is.False);
                Assert.That(editorViewModel.CanExport, Is.False);
                Assert.That(editorViewModel.Hotspots, Has.Exactly(1).Items);
            });
        }

        [AvaloniaTest]
        public async Task ImportConfigNoHotspotsTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(true)
            {
                Config = new Config(MockPythonProxy.CalibrationResult, new List<Hotspot>())
            };
            var pythonHandler = new MockPythonHandler();
            IEditorViewModel editorViewModel = new EditorViewModel(navigator, fileHandler, pythonHandler, VMProvider);

            editorViewModel.AddHotspot();
            Assert.That(editorViewModel.IsSaved, Is.False);

            var imported = await editorViewModel.ImportConfig("test.zip");

            Assert.Multiple(() =>
            {
                Assert.That(imported, Is.True);
                Assert.That(editorViewModel.IsSaved);
                Assert.That(editorViewModel.CanExport, Is.True);
                Assert.That(editorViewModel.Hotspots, Is.Empty);
            });
        }

        [AvaloniaTest]
        [TestCase(true, TestName = "Successful")]
        [TestCase(false, TestName = "Unsuccessful")]
        public async Task ExportConfigTest(bool expectedSuccess)
        {
            const string exportPath = "test";
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(expectedSuccess)
            {
                Delay = 500
            };
            var pythonHandler = new MockPythonHandler();
            IEditorViewModel editorViewModel = new EditorViewModel(navigator, fileHandler, pythonHandler, VMProvider);

            editorViewModel.AddHotspot();
            Assert.That(editorViewModel.IsSaved, Is.False);

            var task = editorViewModel.ExportConfig(exportPath);
            Assert.That(editorViewModel.IsExporting, Is.True);

            var exported = await task;
            Assert.That(editorViewModel.IsExporting, Is.False);

            Assert.Multiple(() =>
            {
                Assert.That(exported, Is.EqualTo(expectedSuccess));
                Assert.That(editorViewModel.IsSaved, Is.False);
                Assert.That(editorViewModel.CanExport, Is.False);
                Assert.That(
                    fileHandler.ExportedZips,
                    Is.EquivalentTo(new[] { Path.Combine(exportPath, IEditorViewModel.ExportFileName) })
                );
            });
        }

        [AvaloniaTest]
        public async Task ExportConfigExceptionTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new IOException());
            var pythonHandler = new MockPythonHandler();
            IEditorViewModel editorViewModel = new EditorViewModel(navigator, fileHandler, pythonHandler, VMProvider);

            editorViewModel.AddHotspot();
            Assert.Multiple(() =>
            {
                Assert.That(editorViewModel.IsSaved, Is.False);
                Assert.That(editorViewModel.CanExport, Is.False);
            });

            var exported = await editorViewModel.ExportConfig("test");

            Assert.Multiple(() =>
            {
                Assert.That(exported, Is.False);
                Assert.That(editorViewModel.IsSaved, Is.False);
                Assert.That(editorViewModel.CanExport, Is.False);
            });
        }

        [AvaloniaTest]
        public void ShowHideCalibrationMarkersTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            var pythonHandler = new MockPythonHandler();
            IEditorViewModel editorViewModel = new EditorViewModel(navigator, fileHandler, pythonHandler, VMProvider);
            Assert.That(navigator.AreArUcoMarkersVisible, Is.False);

            editorViewModel.ShowCalibrationMarkers();
            Assert.That(navigator.AreArUcoMarkersVisible, Is.True);

            editorViewModel.HideCalibrationMarkers();
            Assert.That(navigator.AreArUcoMarkersVisible, Is.False);
        }

        [AvaloniaTest]
        public async Task CalibrateCameraTest()
        {
            var positionsBuilder = ImmutableDictionary.CreateBuilder<int, Point>();
            for (var i = 0; i < 5; i++)
                positionsBuilder.Add(i, new Point(i, i));

            var positions = positionsBuilder.ToImmutable();
            var navigator = new MockNavigator(positions);
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            var pythonHandler = new MockPythonHandler()
            {
                Delay = 500
            };
            IEditorViewModel editorViewModel = new EditorViewModel(navigator, fileHandler, pythonHandler, VMProvider);

            var task = editorViewModel.CalibrateCamera();
            Assert.That(editorViewModel.IsCalibrating, Is.True);

            var result = await task;
            Assert.That(editorViewModel.IsCalibrating, Is.False);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(editorViewModel.IsSaved, Is.False);
                Assert.That(editorViewModel.CanExport, Is.False);
            });

            await editorViewModel.SaveConfig();
            Assert.That(fileHandler.Config.HomographyMatrix, Is.EquivalentTo(MockPythonProxy.CalibrationResult));
        }

        [AvaloniaTest]
        public async Task CalibrateCameraNullTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(true);
            var pythonHandler = new MockPythonHandler();

            IEditorViewModel editorViewModel = new EditorViewModel(navigator, fileHandler, pythonHandler, VMProvider);
            Assert.That(await editorViewModel.CalibrateCamera(), Is.False);
            Assert.Multiple(() =>
            {
                Assert.That(editorViewModel.IsSaved, Is.True);
                Assert.That(editorViewModel.CanExport, Is.False);
            });
        }

        [AvaloniaTest]
        public void CloseEditorTest()
        {
            var navigator = new MockNavigator();
            var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
            var pythonHandler = new MockPythonHandler();
            navigator.OpenEditor();
            Assert.That(navigator.IsEditorOpen, Is.True);

            IEditorViewModel editorViewModel = new EditorViewModel(navigator, fileHandler, pythonHandler, VMProvider);

            editorViewModel.CloseEditor();

            Assert.That(navigator.IsEditorOpen, Is.False);
        }

        /// <summary>
        /// Asserts that the <see cref="IEditorViewModel.PositionEditor" /> has the expected values.
        /// </summary>
        /// <param name="editorViewModel">The checked <see cref="IEditorViewModel" />.</param>
        /// <param name="expectedSelectedCoord">The expected coordinates of the selected hotspot.</param>
        /// <param name="expectedUnselectedCoord">The expected coordinates of unselected hotspots.</param>
        private static void AssertPositionEditor(
            IEditorViewModel editorViewModel,
            Coord expectedSelectedCoord,
            IEnumerable<Coord> expectedUnselectedCoord
        )
        {
            var positionEditor = editorViewModel.PositionEditor;
            Assert.Multiple(() =>
            {
                Assert.That(positionEditor, Is.Not.Null);
                Assert.That(positionEditor.X, Is.EqualTo(expectedSelectedCoord.X));
                Assert.That(positionEditor.Y, Is.EqualTo(expectedSelectedCoord.Y));
                Assert.That(positionEditor.R, Is.EqualTo(expectedSelectedCoord.R));
                Assert.That(positionEditor.UnselectedHotspots, Is.EquivalentTo(expectedUnselectedCoord));
            });
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
        #region HasEquivalentHotspots

        /// <summary>
        /// Compares two collections of <see cref="IEditorHotspotViewModel" />s for equality by value.
        /// </summary>
        public static CollectionItemsEqualConstraint HasEquivalentHotspots(
            IEnumerable<IEditorHotspotViewModel> expectedCollection
        ) => Is.EquivalentTo(expectedCollection)
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
    internal abstract class Has : NUnit.Framework.Has
    {
        /// <inheritdoc cref="NUnitExtensions.HasEquivalentHotspots(IEnumerable{IEditorHotspotViewModel})" />
        public static CollectionItemsEqualConstraint EquivalentHotspots(
            IEnumerable<IEditorHotspotViewModel> expectedCollection
        ) => NUnitExtensions.HasEquivalentHotspots(expectedCollection);
    }
}

#endregion
