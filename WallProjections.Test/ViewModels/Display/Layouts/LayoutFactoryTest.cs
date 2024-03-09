using WallProjections.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.Test.ViewModels.Display.Layouts;

/// <summary>
/// A generic test for a layout factory.
/// </summary>
/// <typeparam name="TLayout">The type of layout the factory produces.</typeparam>
/// <typeparam name="TLayoutFactory">The type of the factory.</typeparam>
public abstract class LayoutFactoryTest<TLayout, TLayoutFactory>
    where TLayout : Layout
    where TLayoutFactory : LayoutFactory, new()
{
    /// <summary>
    /// Asserts that the factory correctly identifies if the input media is compatible with the layout.
    /// </summary>
    /// <param name="testCase">
    /// A tuple with the following elements:
    /// <ul>
    ///     <li><b>inputMedia</b> - The input media supplied to <see cref="LayoutFactory.IsCompatibleData" />.</li>
    ///     <li><b>expected</b> - The expected result of <see cref="LayoutFactory.IsCompatibleData" />.</li>
    /// </ul>
    /// </param>
    protected void IsCompatibleDataTestBody((Hotspot.Media inputMedia, bool expected) testCase)
    {
        var (inputMedia, expected) = testCase;
        var factory = new TLayoutFactory();

        Assert.That(factory.IsCompatibleData(inputMedia), Is.EqualTo(expected));
    }

    /// <summary>
    /// Asserts that the factory correctly creates a layout from the input media,
    /// or throws an exception if it is not compatible.
    /// </summary>
    /// <param name="testCase">
    /// A tuple with the following elements:
    /// <ul>
    ///     <li><b>inputMedia</b>: The input media supplied to <see cref="LayoutFactory.CreateLayout" />.</li>
    ///     <li>
    ///         <b>compatible</b>: Whether the creation should succeed
    ///                            <i>(see <see cref="LayoutFactory.IsCompatibleData" />)</i>.
    ///     </li>
    /// </ul>
    /// </param>
    /// <param name="layoutAssertions">
    /// Assertions done on the layout after creation (if it is supposed to succeed).
    /// The action provides the newly created layout and the input media.
    /// </param>
    protected void CreateLayoutTestBody(
        (Hotspot.Media inputMedia, bool compatible) testCase,
        Action<TLayout, Hotspot.Media> layoutAssertions
    )
    {
        var (inputMedia, compatible) = testCase;
        var vmProvider = new MockViewModelProvider();
        var factory = new TLayoutFactory();

        if (!compatible)
        {
            Assert.That(() => factory.CreateLayout(vmProvider, inputMedia), Throws.ArgumentException);
            return;
        }

        var layout = factory.CreateLayout(vmProvider, inputMedia);
        Assert.That(layout, Is.TypeOf<TLayout>());
        layoutAssertions((TLayout)layout, inputMedia);
    }
}
