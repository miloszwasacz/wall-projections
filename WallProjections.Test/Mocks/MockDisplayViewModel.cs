using WallProjections.ViewModels;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.Test.Mocks;

/// <summary>
/// A mock of <see cref="DisplayViewModel"/> for injecting into <see cref="MainWindowViewModel"/>
/// </summary>
public class MockDisplayViewModel : IDisplayViewModel
{
    /// <summary>
    /// A stub for <see cref="Description"/> for testing purposes
    /// </summary>
    public const string DefaultDescription = "Default Description: ";

    /// <summary>
    /// A stub for <see cref="VideoViewModel"/>'s constructor for testing purposes
    /// </summary>
    private const string DefaultVideoPath = "Default/Video/Path.mp4";

    /// <summary>
    /// The ID of an artifact the viewmodel should display data about
    /// </summary>
    private readonly string _id;

    /// <summary>
    /// Returns <see cref="DefaultDescription"/> + the ID that the viewmodel was constructed with
    /// </summary>
    public string Description => DefaultDescription + _id;

    /// <summary>
    /// Returns a <see cref="MockVideoViewModel"/> with <see cref="DefaultVideoPath"/> as its video path
    /// </summary>
    public IVideoViewModel? VideoViewModel { get; } = new MockVideoViewModel(DefaultVideoPath);

    /// <summary>
    /// A mock of <see cref="DisplayViewModel"/> for injecting into <see cref="MainWindowViewModel"/>
    /// </summary>
    /// <param name="id">The ID of an artifact the viewmodel should display data about</param>
    public MockDisplayViewModel(string id)
    {
        _id = id;
    }
}
