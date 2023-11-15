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
    def __init__(self, id, x, y, eventHandler, radius=0.03, unpressedColor=(0, 255, 0), pressedColor = (255, 0, 0)):
        self.id = id
        self._x = x
        self._y = y
        self._radius = radius
        self._unpressedColor = unpressedColor
        self._pressesdColor = pressedColor
        self._eventHandler = eventHandler
        self._button = _Button()
    
    def onScreenXY(self, width, height):
        return (int(width*self._x), int(height*self._y))
    
    def onScreenRadius(self):
        return int(1000*self._radius)
    
    def onScreenColor(self):
        """
        Returns a color RGB tuple between pressed and unpressed color depending on buttons pressedAmount
        """
        pressedAmount = self._button.getPressedAmount()
        return(
            int((1 - pressedAmount)* self._unpressedColor[0] + pressedAmount * self._pressesdColor[0]),
            int((1 - pressedAmount)* self._unpressedColor[1] + pressedAmount * self._pressesdColor[1]),
            int((1 - pressedAmount)* self._unpressedColor[2] + pressedAmount * self._pressesdColor[2]),     
        )

    def update(self, points):
        #check if any fingertips inside of HotSpot
        isPressed = False
        for point in points:
            if self._isPointInside(point):
                isPressed = True


        if isPressed:
            if self._button.press():
                #if button has been pressed for long enough send event
                self._eventHandler.OnPressDetected(self.id)
        else:
            self._button.unPress()
        

    def _isPointInside(self, point):
        """
        Returns true if given point is inside hotspot
        """
        squaredDist = (self._x - point.x)**2 + (self._y - point.y)**2
        return squaredDist <= self._radius**2
