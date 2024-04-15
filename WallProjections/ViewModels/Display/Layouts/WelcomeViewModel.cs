namespace WallProjections.ViewModels.Display.Layouts;

/// <summary>
/// A viewmodel for a view that displays a welcome message.
/// </summary>
public class WelcomeViewModel : DescriptionViewModel
{
    //TODO Localized strings?
    internal const string WelcomeTitle = "Select a hotspot";
    internal const string WelcomeMessage = "Please select a hotspot to start";

    /// <summary>
    /// Creates a new <see cref="WelcomeViewModel" />.
    /// </summary>
    public WelcomeViewModel() : base(WelcomeTitle, WelcomeMessage, null)
    {
    }
}
