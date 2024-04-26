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
        VideoCapture.DEFAULT_TARGET = "Assets/TODO.mp4"

    def test_hotspot_detection_video(self):
        class TestEventHandler(EventHandler):
            def __init__(self):
                self.event_record = []

            def OnHotspotPressed(self, hotspot_id):
                self.event_record.append(("press", hotspot_id))

            def OnHotspotUnpressed(self, hotspot_id):
                self.event_record.append(("unpress", hotspot_id))

        event_handler = TestEventHandler()
        calibration_matrix = npnet.asNetArray(np.identity(3))  # TODO
        hotspot_coords_str = "TODO"
        thread = threading.Thread(target=hotspot_detection,
                                  args=(event_handler, calibration_matrix, hotspot_coords_str))
        thread.start()
        time.sleep(1)  # todo length of video at 30 fps + a few seconds
        stop_hotspot_detection()
        assert not thread.is_alive()  # not what we're testing but just checking the thread is finished
        self.assertListEqual(event_handler.event_record, [])  # TODO write in actual events in video


if __name__ == '__main__':
    unittest.main()
