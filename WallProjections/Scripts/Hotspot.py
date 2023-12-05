import math
import time

BUTTON_COOLDOWN = 1.5
"""The number of seconds for which the finger must be over the hotspot until the 
button activates. Once the finger leaves the hotspot, it also takes this amount
of time to \"cool down\"."""


class _Button:
    """
    The _Button class is used to keep track if a HotSpot is activated
    """

    def __init__(self):
        self.finger_is_over = False
        """Whether or not a finger is touching the HotSpot."""

        self.prev_time = time.time()
        """The last time when `finger_is_over` switched value."""

        self.prev_pressed_amount = 0
        """The value of `get_pressed_amount` at the last time when `finger_is_over` switched value."""

    def press(self):
        """
        Called every frame the HotSpot is pressed,
        returns a Bool indicating if the button has been pressed for long enough.
        """
        if not self.finger_is_over:
            self.prev_pressed_amount = self.get_pressed_amount()
            self.prev_time = time.time()
            self.finger_is_over = True

        return self.get_pressed_amount() == 1

    def un_press(self):
        """
        Called every frame the HotSpot is not pressed.
        """
        if self.finger_is_over:
            self.prev_pressed_amount = self.get_pressed_amount()
            self.prev_time = time.time()
            self.finger_is_over = False

    def get_pressed_amount(self):
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

    def __init__(self, hotspot_id, x, y, event_listener, radius=0.03):
        self.id = hotspot_id
        self._x = x
        self._y = y
        self._radius = radius
        self._event_listener = event_listener
        self._button = _Button()
        self._is_activated = False
        self._time_activated = 0

    def draw(self, img, cv2, width, height):
        # Draw outer ring
        cv2.circle(img, self._onscreen_xy(width, height), self._onscreen_radius(), (255, 255, 255), thickness=2)

        # Draw inner circle
        if self._button.get_pressed_amount() != 0:
            cv2.circle(img, self._onscreen_xy(width, height), self._inner_radius(), (255, 255, 255), thickness=-1)

    def _onscreen_xy(self, width, height):
        """
        Takes in screen resolution and outputs pixel coordinates of hotspot
        """

        return int(width * self._x), int(height * self._y)

    def _onscreen_radius(self):
        """
        Returns on screen pixel radius from "mediapipes" radius
        """
        return int(800 * self._radius) #Const value 800 gives a visual HotSpot with a slightly larger "hitbox" than drawn ring

    def _inner_radius(self):
        radius = self._onscreen_radius() * self._button.get_pressed_amount()
        if self._is_activated:
            radius += math.sin((time.time() - self._time_activated) * 3) * 6 + 6
        return int(radius)

    def update(self, points):
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

    def _is_point_inside(self, point):
        """
        Returns true if given point is inside hotspot
        """
        squared_dist = (self._x - point.x) ** 2 + (self._y - point.y) ** 2
        return squared_dist <= self._radius ** 2

    def activate(self):
        self._time_activated = time.time()
        self._is_activated = True

    def deactivate(self):
        self._is_activated = False
