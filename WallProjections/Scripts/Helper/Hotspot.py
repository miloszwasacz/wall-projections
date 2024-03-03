
import time
from typing import NamedTuple, Tuple
import cv2
import math

from calibration import Calibrator
from EventHandler import EventHandler


class Hotspot:
    """
    Represents a hotspot. A hotspot is a circular area on the screen that can be pressed.
    """

    def __init__(self, id: int, proj_pos: Tuple[int, int], calibrator: Calibrator, event_listener: EventHandler, radius: float = 0.03):
        self.id: int = id
        self._norm_pos: Tuple[float, float] = calibrator.proj_to_norm(proj_pos)
        self._radius: float = radius
        self._event_handler: EventHandler = event_listener
        self._prev_fingertip_inside = False

    def update(self, fingertips: list[tuple[float, float]]) -> None:
        fingertip_inside = False

        for fingertip in fingertips:
            if self._is_point_inside(fingertip):
                fingertip_inside = True
                break

        #TODO: while this works, fingertips tend to stutter and disapear often we should add some padding to prevent loads of unneeded calls
        # ie a change of state must have happened for 0.1 secounds before we notify event handler

        #Case1: fingertip inside hotspot on last update, now no fingertips inside
        if self._prev_fingertip_inside == True and fingertip_inside == False:
            EventHandler.on_hotspot_unpressed(self.id)
        #Case2: no fingertips inside hotspot on last update, now fingertips inside
        if self._prev_fingertip_inside == False and fingertip_inside == True:
            EventHandler.on_hotspot_pressed(self.id)
        #Case3: nothings changed do nothing
        
        self._prev_fingertip_inside = fingertip_inside
        


    def _is_point_inside(self, point: tuple[float, float]) -> bool:
        """
        Returns true if given point is inside hotspot
        """
        squared_dist = (self._norm_pos[0] - point.x) ** 2 + (self._norm_pos[1] - point.y) ** 2
        return squared_dist <= self._radius ** 2

