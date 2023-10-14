using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvaloniaApplication1.ViewModels;
using System;

namespace AvaloniaApplication1;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        return data is ViewModelBase;
    }
}