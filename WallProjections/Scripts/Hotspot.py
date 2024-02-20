
import time
from typing import NamedTuple, Tuple
import cv2
import math

from Calibrator import Calibrator
from EventListener import EventListener


BUTTON_COOLDOWN: float = 1.5
"""The number of seconds for which the finger must be over the hotspot until the 
button activates. Once the finger leaves the hotspot, it also takes this amount
of time to \"cool down\"."""


class Point(NamedTuple):
    x: float
    y: float


class _Button:
    """
    The _Button class is used to keep track if a HotSpot is activated
    """

    def __init__(self):
        self.finger_is_over: bool = False
        """Whether or not a finger is touching the HotSpot."""

        self.prev_time: float = time.time()
        """The last time when `finger_is_over` switched value."""

        self.prev_pressed_amount: float = 0
        """The value of `get_pressed_amount` at the last time when `finger_is_over` switched value."""

    def press(self) -> bool:
        """
        Called every frame the HotSpot is pressed,
        returns a Bool indicating if the button has been pressed for long enough.
        """
        if not self.finger_is_over:
            self.prev_pressed_amount = self.get_pressed_amount()
            self.prev_time = time.time()
            self.finger_is_over = True

        return self.get_pressed_amount() == 1

    def un_press(self) -> None:
        """
        Called every frame the HotSpot is not pressed.
        """
        if self.finger_is_over:
            self.prev_pressed_amount = self.get_pressed_amount()
            self.prev_time = time.time()
            self.finger_is_over = False

    def get_pressed_amount(self) -> float:
        """
        Returns a value between 0 (fully unpressed) and 1 (fully pressed.)
        """
        time_since_last_change = time.time() - self.prev_time
        if self.finger_is_over:
            return min(1.0, self.prev_pressed_amount + time_since_last_change / BUTTON_COOLDOWN)
        else:
            return max(0.0, self.prev_pressed_amount - time_since_last_change / BUTTON_COOLDOWN)


class Hotspot:
    """
    Represents a hotspot. A hotspot is a circular area on the screen that can be pressed.
    """

    def __init__(self, id: int, proj_pos: Tuple[int, int], calibrator: Calibrator, event_listener: EventListener, radius: float = 0.03):
        self.id: int = id
        self._proj_pos: Tuple[int, int] = proj_pos
        self._norm_pos: Tuple[float, float] = calibrator.proj_to_norm(proj_pos)
        self._radius: float = radius
        self._event_listener: EventListener = event_listener
        self._button: _Button = _Button()
        self._is_activated: bool = False
        self._time_activated: float = 0

    def draw(self, img) -> None:
        img_width = img.shape[1]
        img_height = img.shape[0]

        # Draw outer ring
        cv2.circle(img, self._proj_pos, self._onscreen_radius(), (255, 255, 255),
                   thickness=2)

        # Draw inner circle
        if self._button.get_pressed_amount() != 0:
            cv2.circle(img, self._proj_pos, self._inner_radius(), (255, 255, 255),
                       thickness=-1)

    def _onscreen_radius(self) -> int:
        """
        Returns on screen pixel radius from "mediapipes" radius
        """
        # Const value 800 gives a visual HotSpot with a slightly larger "hitbox" than drawn ring
        return int(800 * self._radius)

    def _inner_radius(self) -> int:
        radius = self._onscreen_radius() * self._button.get_pressed_amount()
        if self._is_activated:
            radius += math.sin((time.time() - self._time_activated) * 3) * 6 + 6
        return int(radius)

    def update(self, points: list[Point]) -> bool:
        if self._is_activated:
            pass

        else:  # not activated
            # check if any fingertips inside of hotspot
            finger_inside = False
            for point in points:
                if self._is_point_inside(point):
                    finger_inside = True

            if finger_inside:
                if self._button.press():
                    # button has been pressed for long enough
                    self.activate()
                    return True
            else:
                self._button.un_press()

        return False

    def _is_point_inside(self, point: Point) -> bool:
        """
        Returns true if given point is inside hotspot
        """
        squared_dist = (self._proj_pos[0] - point.x) ** 2 + (self._proj_pos[1] - point.y) ** 2
        return squared_dist <= self._radius ** 2

    def activate(self) -> None:
        self._time_activated = time.time()
        self._is_activated = True

    def deactivate(self) -> None:
        self._is_activated = False
