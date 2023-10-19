import RPi.GPIO as GPIO
import time

GPIO.setwarnings(True)
GPIO.setmode(GPIO.BCM)

#The GPIO pins connected to buttons
connectedPins = [2, 3, 4, 17]

for pin in connectedPins:
    GPIO.setup(pin, GPIO.IN, pull_up_down=GPIO.PUD_UP)

while True:
    for pin in connectedPins:
        if GPIO.input(pin) == GPIO.LOW:
            print("Button connected to pin",pin,"was pressed!")
            time.sleep(0.1)