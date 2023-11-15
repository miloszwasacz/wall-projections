import time

TIMEREQUIRED=1.5 
GRACEPERIOD=0.5

class _Button:
    """
    The _Button class is used to keep track if a HotSpot is activated:
    - If pressed for TIMEREQUIRED secounds, we return True, otherwise return False
    - If unpressed and then repressed within GRACEPERIOD we continue counting up towards TIMEREQUIRED
    otherwise we reset our timer
    - During unpressed periods (even if the button is repressed within GRACEPERIOD) our timer doeesn't count up
    """

    def __init__(self):

        self.timeAtPressed = time.time() #time.time() when button last pressed on unpressed
        self.isPressed = False
        self.newTimeRequired = TIMEREQUIRED # the time required for successful press, including the time of unpresses
    
    
    def _pressedTime(self):
        return time.time() - self.timeAtPressed

    def press(self):
        """
        Called every frame the HotSpot is pressed,
        returns a Bool indicating if the button has been pressed for long enough
        """

        if not self.isPressed: #If previously unpressed:
            if self._pressedTime() < GRACEPERIOD:#If within grace period:
                self.newTimeRequired +=  self._pressedTime() #Increase time required by time spent outside of HotSpot
                self.timeAtPressed = time.time()
            else: #Otherwise reset:
                self.newTimeRequired = TIMEREQUIRED
                self.timeAtPressed = time.time()
            self.isPressed = True
        else: #If previosuly pressed:
            if self._pressedTime() > self.newTimeRequired: #If pressed for long enough:
                self.newTimeRequired = TIMEREQUIRED
                self.timeAtPressed = time.time()
                self.isPressed = True
                return True
        return False

    def unPress(self):
        """
        Called every frame the HotSpot is not pressed
        """
        if self.isPressed: #If previously pressed:
            self.newTimeRequired -= self._pressedTime() #Decrease time required by time spent inside of HotSpot
            self.timeAtPressed = time.time()
            self.isPressed = False

    def getPressedAmount(self):
        """
        Returns a value between 0 (fully unpressed) and 1 (fully pressed.)
        """
        if self.isPressed:
            return 1
        return 0

        
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