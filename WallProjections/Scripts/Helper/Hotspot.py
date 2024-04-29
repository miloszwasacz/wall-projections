from typing import Tuple

from Scripts.Helper.EventHandler import EventHandler
from Scripts.Helper.logger import get_logger

logger = get_logger()

class Hotspot:
    """
    Represents a hotspot. A hotspot is a circular area on the screen that can be pressed.
    """

    def __init__(
            self,
            hotspot_id: int,
            proj_pos: Tuple[float, float],
            event_listener: EventHandler,
            radius: float = 0.03
    ):
        self.id: int = hotspot_id
        self._proj_pos: Tuple[float, float] = proj_pos
        self._radius: float = radius
        self._event_handler: EventHandler = event_listener
        self._prev_fingertip_inside = False

    def update(self, fingertips: list[tuple[float, float]]) -> None:
        fingertip_inside = False

        for fingertip in fingertips:
            if self._is_point_inside(fingertip):
                fingertip_inside = True
                break

        # TODO: While this works, fingertips tend to stutter and disappear often.
        #       We should add some padding to prevent loads of unneeded calls,
        #       i.e. a change of state must have happened for 0.1 seconds before we notify event handler.

        # Case1: fingertip inside hotspot on last update, now no fingertips inside
        if self._prev_fingertip_inside and not fingertip_inside:
            logger.info(f"Hotspot {self.id} unpressed.")
            self._event_handler.OnHotspotUnpressed(self.id)

        # Case2: no fingertips inside hotspot on last update, now fingertips inside
        if not self._prev_fingertip_inside and fingertip_inside:
            logger.info(f"Hotspot {self.id} pressed.")
            self._event_handler.OnHotspotPressed(self.id)

        # Case3: nothings changed do nothing
        self._prev_fingertip_inside = fingertip_inside

    def _is_point_inside(self, point) -> bool:
        """
        Returns true if given point is inside hotspot
        """
        squared_dist = (self._proj_pos[0] - point[0]) ** 2 + (self._proj_pos[1] - point[1]) ** 2
        return squared_dist <= self._radius ** 2
