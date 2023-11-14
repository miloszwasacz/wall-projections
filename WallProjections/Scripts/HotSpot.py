import time
class Button:
    """
    The button class is used to keep track if a HotSpot is pressed:
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