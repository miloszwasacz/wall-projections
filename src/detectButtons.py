import RPi.GPIO as GPIO
import time

GPIO.setwarnings(True)
GPIO.setmode(GPIO.BCM)

#The GPIO pins connected to buttons
connectedPins = [2, 3, 4, 17]

for pin in connectedPins:
    GPIO.setup(pin, GPIO.IN, pull_up_down=GPIO.PUD_UP)

def button_pressed(pin):
    print("Button connected to pin",pin,"was pressed!")

for pin in connectedPins:
    GPIO.add_event_detect(pin, GPIO.FALLING, callback=button_pressed, bouncetime=200)

while True: #Let program keep running
    time.sleep(0.1)  