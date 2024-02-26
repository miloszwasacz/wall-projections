from typing import List
import numpy as np
from importme import import_me
import time


def calibrate(dict) -> np.ndarray:
    """
    Takes a dictionary of id's to coords
    returns a 3x3 np array of 32 bit numpy floats
    """

    return np.array(calibrate2(dict), dtype=np.float32)

def calibrate2(dict) -> List[List[float]]:
    """
    Takes a dictionary of id's to coords
    returns a 3x3 List of 64 bit python standard floats 

    """
    import_me() #test importing works

    
    return [[0.0, 0.1, 0.2], [1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] 




def hotspot_detection(event_handler) -> None:
    """
    Given hotspot projector coords, a transformation matrix and an event_handler
    calls events when hotspots are pressed or unpressed
    """

    for i in range(30):
        event_handler.hotspot_pressed(i%3)
        time.sleep(1)
        event_handler.hotspot_unpressed(i%3)
        time.sleep(1)

if __name__ == "__main__":
    class Event_Handler:
        def hotspot_pressed(self, id):
            print("hotspot pressed", id)
        
        def hotspot_unpressed(self, id):
            print("hotspot unpressed", id)
    
    print(calibrate)
    print(calibrate2)
    hotspot_detection(Event_Handler())