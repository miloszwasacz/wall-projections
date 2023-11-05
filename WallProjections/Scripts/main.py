# This is the main entrypoint for C# to call into Python
import RPi.GPIO as GPIO
import time

GPIO.setwarnings(True)
GPIO.setmode(GPIO.BCM)


def detect_buttons(event_handler):
    

    pinToButtonId = { #matches GPIO pins on the raspberry pi to the value passed to the dotnet program
        2: 1,
        3: 2,
        4: 3,
        17: 4
    }

    def buttonPressed(id):
        event_handler.OnPressDetected(id)

    for pin in pinToButtonId.keys(): #Foreach pin
        GPIO.setup(pin, GPIO.IN, pull_up_down=GPIO.PUD_UP) #Setup pin on pi
        GPIO.add_event_detect(pin, GPIO.FALLING, callback=buttonPressed(pinToButtonId[pin]), bouncetime=200) #If pin falls (buttons pressed) call buttonPressed function

    try:
        while True: #Let program keep running
            time.sleep(0.1)
    except KeyboardInterrupt:
        GPIO.cleanup()
