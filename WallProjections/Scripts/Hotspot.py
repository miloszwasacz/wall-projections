from typing import NamedTuple
from EventListener import EventListener

import cv2
import math
import time


HOTSPOT_COOLDOWN: float = 1.5
"""The number of seconds for which the finger must be over the hotspot until it activates. Once the finger leaves the 
hotspot, it also takes this amount of time to \"cool down\"."""


class Point(NamedTuple):
    """
    A simple class to keep track of point coordinates.
    """
    x: float
    y: float


class _ProgressTimer:
    """
    Used to keep track of the elapsed time (progress) towards activating a hotspot.

    Keeps track of `progress`, a value in [0, 1] indicating how much of the provided `duration` has been completed.
    """

    def __init__(self, duration: float):
        """
        :param duration: When the ``ProgressTimer`` reaches this time, the progress is 1.
        """

        self._counting_up: bool = False
        """The ``ProgressTimer`` will count up if this property is set to True; otherwise it will count down."""

        self._duration = duration
        """When the ``ProgressTimer`` reaches this time, the progress is 1."""

        self._time_last_switched: float = time.time()
        """The clock time when ``counting_up`` last switched value."""

        self._progress_last_switched: float = 0
        """The value of ``get_progress`` at the last time when ``counting_up`` switched value."""

    def count_up(self) -> None:
        """
        Make the ``ProgressTimer`` count upwards (increasing progress).

        It will keep on counting upwards until it reaches full progress or ``count_down`` is called. If it is already
        counting up, this method does nothing.
        """
        if not self._counting_up:
            self._progress_last_switched = self.get_progress()
            self._time_last_switched = time.time()
            self._counting_up = True

    def count_down(self) -> None:
        """
        Make the ``ProgressTimer`` count downwards (decreasing progress).

        It will keep on counting downwards until it reaches zero progress or ``count_up`` is called. If it is already
        counting down, this method does nothing.
        """
        if self._counting_up:
            self._progress_last_switched = self.get_progress()
            self._time_last_switched = time.time()
            self._counting_up = False

    def get_progress(self) -> float:
        """
        :return: a value in [0, 1] representing the progress towards the specified `duration`.
        """
        time_since_last_switch = time.time() - self._time_last_switched
        if self._counting_up:
            return min(1.0, self._progress_last_switched + time_since_last_switch / self._duration)
        else:
            return max(0.0, self._progress_last_switched - time_since_last_switch / self._duration)


class Hotspot:
    """
    Represents a hotspot. A hotspot is a circular area on the screen that can be pressed.
    """

    def __init__(self, hotspot_id: int, x: float, y: float, event_listener: EventListener, radius: float = 0.03):
        self.id: int = hotspot_id
        self._x: float = x
        self._y: float = y
        self._radius: float = radius
        self._event_listener: EventListener = event_listener
        self._progress_timer: _ProgressTimer = _ProgressTimer(HOTSPOT_COOLDOWN)
        self._is_activated: bool = False
        self._time_activated: float = 0

    def draw(self, img: cv2.typing.MatLike) -> None:
        img_width = img.shape[1]
        img_height = img.shape[0]

        # Draw outer ring
        cv2.circle(img, self._onscreen_xy(img_width, img_height), self._onscreen_radius(), (255, 255, 255),
                   thickness=2)

        # Draw inner circle
        if self._progress_timer.get_progress() != 0:
            cv2.circle(img, self._onscreen_xy(img_width, img_height), self._inner_radius(), (255, 255, 255),
                       thickness=-1)

    def _onscreen_xy(self, screen_width: int, screen_height: int) -> tuple[int, int]:
        """
        Takes in screen resolution and outputs pixel coordinates of hotspot
        """

        return int(screen_width * self._x), int(screen_height * self._y)

    def _onscreen_radius(self) -> int:
        """
        Returns on screen pixel radius from "mediapipes" radius
        """
        # Const value 800 gives a visual HotSpot with a slightly larger "hitbox" than drawn ring
        return int(800 * self._radius)

    def _inner_radius(self) -> int:
        radius = self._onscreen_radius() * self._progress_timer.get_progress()
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
                self._progress_timer.count_up()
                if self._progress_timer.get_progress() == 1:
                    # button has been pressed for long enough
                    self.activate()
                    return True
            else:
                self._progress_timer.count_down()

        return False

    def _is_point_inside(self, point: Point) -> bool:
        """
        Returns true if given point is inside hotspot
        """
        squared_dist = (self._x - point.x) ** 2 + (self._y - point.y) ** 2
        return squared_dist <= self._radius ** 2

    def activate(self) -> None:
        self._time_activated = time.time()
        self._is_activated = True

    def deactivate(self) -> None:
        self._is_activated = False
