# This is the main entrypoint for C# to call into Python
import RPi.GPIO as GPIO
import time

GPIO.setwarnings(True)
GPIO.setmode(GPIO.BCM)


def detect_buttons(event_handler):  # This function is called by Program.cs
    pinToButtonId = {  # Matches GPIO pins on the Pi to the value passed to Program.cs
        2: 1,
        3: 2,
        4: 3,
        17: 4
    }

    def buttonPressed(pin):
        id = pinToButtonId[pin]
        print("button press", id)
        event_handler.OnPressDetected(id)

    for pin, id in pinToButtonId.items():  # Foreach pin
        GPIO.setup(pin, GPIO.IN, pull_up_down=GPIO.PUD_UP)  # Setup pin on Pi
        GPIO.add_event_detect(pin, GPIO.FALLING, callback=buttonPressed, bouncetime=200)  # If pin falls (buttons pressed) call buttonPressed function

    try:
        while True:  # Let program keep running
            time.sleep(0.1)
    except KeyboardInterrupt:
        GPIO.cleanup()

if __name__ == "__main__":
    class EventHandler:
        def OnPressDetected(self, id):
            print("button pressed", id)
    detect_buttons(EventHandler())