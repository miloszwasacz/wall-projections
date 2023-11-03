# This is the main entrypoint for C# to call into Python
from time import sleep


def simulate_buttons(event_handler):
    i = 0
    sleep(5)
    while True:
        sleep(1)
        event_handler.OnPressDetected(i)
        i += 1
