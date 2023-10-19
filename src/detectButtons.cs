using System.Device.Gpio;
using System.Threading.Tasks;

int[] pins = { 2, 3, 4, 17 };

using var controller = new GpioController();

foreach (int pin in pins)
{
    controller.OpenPin(pin, PinMode.InputPullUp); //Detecting change in voltage for each pin in pins
    controller.RegisterCallbackForPinValueChangedEvent( //If pin is Falling (connected to GND) call function OnPinEvent
    pin,
    PinEventTypes.Falling,
    (sender, args) => OnPinEvent(sender, args, pin));
}

await Task.Delay(Timeout.Infinite); //Wait infinitly to stop code terminating

static void OnPinEvent(object sender, PinValueChangedEventArgs args, int pin)
{
    Console.WriteLine("Pressed pin:" + pin);
    Task.Delay(500).Wait();
}