using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using Avalonia.Interactivity;
using System.Drawing;
using Avalonia.Input;
using System.Device.Gpio;
using System.Threading.Tasks;

namespace WallProjection;

public partial class MainWindow : Window
{


    private readonly Dictionary<int, string> pinToPath = new Dictionary<int, string>
        {
            {2,  "assets/image1.jpg"},
            {3, "assets/image2.jpg" },
            {4, "assets/image1.jpg" },
            {17, "assets/image2.jpg" },
        };

    public MainWindow()
    {
        InitializeComponent();
        displayedImage = this.Find<Avalonia.Controls.Image>("displayedImage");
   
        ShowImage("./assets/image1.jpg");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        using var controller = new GpioController();

        foreach (int pin in pinToPath.Keys)
        {
            Console.WriteLine("made pin:" + pin);
            controller.OpenPin(pin, PinMode.InputPullUp); //Detecting change in voltage for each pin in pins
            controller.RegisterCallbackForPinValueChangedEvent( //If pin is Falling (connected to GND) call function OnPinEvent
            pin,
            PinEventTypes.Falling,
            (sender, args) => ButtonPressed(sender, args, pin));
        }
    }

    private void ButtonPressed(object sender, PinValueChangedEventArgs args, int pin)
    {
        Console.WriteLine("Pressed pin:" + pin);
        if (pinToPath.TryGetValue(pin, out string path))
        {
            this.ShowImage(path);
        }
        else
        {
            Console.Error.WriteLine($"Dictionary doesn't contain path for pin {pin}");
        }
    }


    private void ShowImage(string path)
    {
        try
        {
            displayedImage.IsVisible = true;
            var bitmap = new Avalonia.Media.Imaging.Bitmap(path);
            displayedImage.Source = bitmap;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Error displaying image at path {path}: {e.Message}");
        }
    }
}