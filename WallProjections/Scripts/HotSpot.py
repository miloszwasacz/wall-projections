import time
class _Button:
    """
    The _Button class is used to keep track if a HotSpot is activated:
    - If pressed for TIMEREQUIRED secounds, we return True, otherwise return False
    - If unpressed and then repressed within GRACEPERIOD we continue counting up towards TIMEREQUIRED
    otherwise we reset our timer
    - During unpressed periods (even if the button is repressed within GRACEPERIOD) our timer doeesn't count up
    """

    def __init__(self, TIMEREQUIRED, GRACEPERIOD):
        self.TIMEREQUIRED = TIMEREQUIRED
        self.GRACEPERIOD = GRACEPERIOD

        self.timePressed = None #time.time() when button started pressing
        self.timeUnpressed = None #time.time() when button started unpressing
        self.newTimeRequired = self.TIMEREQUIRED # the time required for successful press, including the time of unpresses
    
    def press(self):
        currentTime = time.time()
        if currentTime - self.timePressed > self.newTimeRequired: #Button has been successfully pressed:
            #reset button
            self.timePressed = None
            self.timeUnpressed = None
            self.newTimeRequired = self.TIMEREQUIRED
            return True
        if self.timePressed == None: #Button not pressed:
            self.timePressed = time.time()
            self.timeUnpressed = None
        elif currentTime - self.timeUnpressed < self.GRACEPERIOD: #Button breifly unpressed:
            self.newTimeRequired += currentTime - self.timeUnpressed
            self.timeUnpressed = None

        return False

    def unPress(self):
        if self.timeUnpressed == None:
            self.timeUnpressed = time.time()

    def getPressedAmount(self):
        return min(0, max (self.timePressed / self.newTimeRequired, 1))
        
class HotSpot():
    """
    The HotSpot class represents an onscreen hotspot which a user interacts with
    """
    def __init__(self, id, x, y, eventHandler, radius=0.03, unpressedColor=(0, 255, 0), pressedColor = (255, 0, 0), TIMEREQUIRED=2.5, GRACEPERIOD=0.8):
        self.id = id
        self._x = x
        self._y = y
        self._radius = radius
        self._unpressedColor = unpressedColor
        self._pressesdColor = pressedColor
        self._eventHandler = eventHandler
        self._button = _Button(TIMEREQUIRED, GRACEPERIOD)
    
    def onScreenX(self, width):
        return width*self._x
    
    def onScreenY(self, height):
        return height*self._y
    
    def onScreenRadius(self):
        return 1000*self._radius
    
    def onScreenColor(self):
        pressedAmount = self._button.getPressedAmount()
        return(
            (1 - pressedAmount)* self._unpressedColor[0] + pressedAmount * self._pressesdColor[0],
            (1 - pressedAmount)* self._unpressedColor[1] + pressedAmount * self._pressesdColor[1],
            (1 - pressedAmount)* self._unpressedColor[2] + pressedAmount * self._pressesdColor[2],     
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
        return squaredDist <= self.radius**2