using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using WallProjections.ViewModels;

namespace WallProjections;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        var name = data?.GetType().FullName!.Replace("ViewModel", "View");

// Used only in unit tests
#if !RELEASE
        name = name?.Replace(".Test.Mocks", "").Replace("Mock", "");
#endif

        var type = name != null ? Type.GetType(name) : null;

        if (type != null)
            return (Control)Activator.CreateInstance(type)!;

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
