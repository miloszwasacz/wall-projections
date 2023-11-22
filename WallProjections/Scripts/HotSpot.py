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
        self.fingerIsOver = False
        """Whether or not a finger is touching the HotSpot"""

        self.prevTime = time.time()
        """The last time when `fingerIsOver` switched value"""

        self.prevPressedAmount = 0
        """The value of `getPressedAmount` at the last time when `fingerIsOver` switched value"""

    def press(self):
        """
        Called every frame the HotSpot is pressed,
        returns a Bool indicating if the button has been pressed for long enough
        """
        if not self.fingerIsOver:
            self.prevPressedAmount = self.getPressedAmount()
            self.prevTime = time.time()
            self.fingerIsOver = True

        return self.getPressedAmount() == 1

    def unPress(self):
        """
        Called every frame the HotSpot is not pressed
        """
        if self.fingerIsOver:
            self.prevPressedAmount = self.getPressedAmount()
            self.prevTime = time.time()
            self.fingerIsOver = False

    def getPressedAmount(self):
        """
        Returns a value between 0 (fully unpressed) and 1 (fully pressed.)
        """
        timeSinceLastChange = time.time() - self.prevTime
        if self.fingerIsOver:
            pressedAmount = min(1.0, self.prevPressedAmount + timeSinceLastChange / BUTTON_COOLDOWN)
        else:
            pressedAmount = max(0.0, self.prevPressedAmount - timeSinceLastChange / BUTTON_COOLDOWN)

        return pressedAmount

        
class HotSpot():
    """
    The HotSpot class represents an onscreen hotspot which a user interacts with
    """
    def __init__(self, id, x, y, eventHandler, radius=0.03):
        self.id = id
        self._x = x
        self._y = y
        self._radius = radius
        self._eventHandler = eventHandler
        self._button = _Button()
        self._isActivated = False
        self._timeActivated = 0
    
    def draw(self, img, cv2, width, height):
        #Draw outer ring
        cv2.circle(img, self._onScreenXY(width, height), self._onScreenRadius(), (255, 255, 255), thickness=2)

        #Draw inner circle
        if self._button.getPressedAmount() != 0:
            cv2.circle(img, self._onScreenXY(width, height), self._innerRadius(), (255,255,255), thickness=-1)

    def _onScreenXY(self, width, height):
        """
        Takes in screen resolution and outputs pixel coordinates of hotspot
        """

        return (int(width*self._x), int(height*self._y))
    
    def _onScreenRadius(self):
        return int(1000*self._radius)
    
    def _innerRadius(self):
        radius = self._onScreenRadius()*self._button.getPressedAmount()
        if self._isActivated:
            radius += math.sin((time.time() - self._timeActivated) * 3) * 6 + 6
        return int(radius)

    def update(self, points):
        if self._isActivated:
            # todo
            pass

        else:  # not activated
            #check if any fingertips inside of HotSpot
            fingerInside = False
            for point in points:
                if self._isPointInside(point):
                    fingerInside = True


            if fingerInside:
                if self._button.press():
                    # button has been pressed for long enough
                    self.activate()
                    return True
            else:
                self._button.unPress()

        return False
        

    def _isPointInside(self, point):
        """
        Returns true if given point is inside hotspot
        """
        squaredDist = (self._x - point.x)**2 + (self._y - point.y)**2
        return squaredDist <= self._radius**2

    def activate(self):
        self._timeActivated = time.time()
        self._isActivated = True

    def deactivate(self):
        self._isActivated = False
