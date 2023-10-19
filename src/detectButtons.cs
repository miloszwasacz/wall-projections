//Based of learn.microsoft.com/en-us/dotnet/iot/tutorials/gpio-input

using System.Device.Gpio;
using System.Threading.Tasks;

int pin = 21;

using var controller = new GpioController();
controller.OpenPin(pin, PinMode.InputPullUp);

controller.RegisterCallbackForPinValueChangedEvent(
    pin,
    PinEventTypes.Falling,
    OnPinEvent);

await Task.Delay(Timeout.Infinite);

static void OnPinEvent(object sender, PinValueChangedEventArgs args)
{
    Console.WriteLine("Pressed!");
}