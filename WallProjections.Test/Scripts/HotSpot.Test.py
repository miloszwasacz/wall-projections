import unittest
import time

from WallProjections.Scripts.HotSpot import _Button
from WallProjections.Scripts.HotSpot import BUTTON_COOLDOWN

BUTTON_COOLDOWN = 1.5

class TestButtons(unittest.TestCase):
    
   
    def continuousPress(self):
        """
        Check if Button is pressed after continous press
        """
        button = _Button()
        button.press()
        time.sleep(BUTTON_COOLDOWN+0.05)
        self.assertEqual(button.getPressedAmount, 1) #assert pressed

    def pressUnpressPress(self):
        """
        Check if Button gets pressed after press unpress press 
        """
        button = _Button()
        button.press()
        time.sleep(BUTTON_COOLDOWN-0.5) #press for 1 secounds
        button.unPress()
        time.sleep(0.5) #unpress for 0.5 secounds
        self.assertNotEqual(button.getPressedAmount, 1) #assert unpressed
        button.press() 
        time.sleep(0.5) #press for 0.5 secounds
        self.assertNotEqual(button.getPressedAmount, 1) #assert unpressed
        time.sleep(0.6)
        self.assertEqual(button.getPressedAmount, 1) #assert pressed

if __name__ == "__main__":
    unittest.main()