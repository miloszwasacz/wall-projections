import threading
import time
import unittest

import numpy as np

from Helper import VideoCapture
from Helper.EventHandler import EventHandler
from Interop import numpy_dotnet_converters as npnet
from hotspot_detection import hotspot_detection, stop_hotspot_detection


class TestHotspotDetection(unittest.TestCase):
    def setUp(self):  # called before every test
        VideoCapture.DEFAULT_TARGET = "Assets/hotspot_test.avi"

    def test_hotspot_detection_video(self):
        class TestEventHandler(EventHandler):
            def __init__(self):
                self.hotspots_pressed = set()

            def OnHotspotPressed(self, hotspot_id):
                self.hotspots_pressed.add(hotspot_id)

            def OnHotspotUnpressed(self, hotspot_id):
                pass

        event_handler = TestEventHandler()
        calibration_matrix = npnet.asNetArray(np.array([[ 5.20479000e+00,  3.15230221e-01, -6.84127477e+02],
                                                        [-1.26843385e-01,  5.48706413e+00, -9.97831632e+02],
                                                        [-1.31811578e-04,  2.86799201e-04,  1.00000000e+00]]))
        hotspot_coords_str = '{"0":[260,211,87],"1":[1682,228,89],"2":[1689,885,93],"3":[240,900,89]}'
        thread = threading.Thread(target=hotspot_detection,
                                  args=(event_handler, calibration_matrix, hotspot_coords_str))
        thread.start()
        time.sleep(20)
        stop_hotspot_detection()
        assert not thread.is_alive()  # not what we're testing but just checking the thread is finished
        print(f"Hotspots pressed: {event_handler.hotspots_pressed}")
        correct_set = {0, 1, 2, 3}
        self.assertSetEqual(event_handler.hotspots_pressed, correct_set)


if __name__ == '__main__':
    unittest.main()
