import numpy as np
from importme import import_me
import time


def calibrate() -> np.ndarray:
    """
    Given a dictionary of ArUco id's to projector coords (the top left pixel)
    returns a transformation matrix (3x3 np array of np 32bit floats)
    """
    import_me()

    #a random 3x3 np array of np 32bit floats, this type will probably need to be changed for C#
    return np.random.rand(3, 3).astype(np.float32) 


def hotspot_detection(event_handler) -> None:
    """
    Given hotspot projector coords, a transformation matrix and an event_handler
    calls events when hotspots are interacted with
    """

    for i in range(30):
        event_handler.hotspot_pressed(i%3)
        time.sleep(1)