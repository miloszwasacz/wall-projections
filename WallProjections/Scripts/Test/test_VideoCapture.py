import time
import unittest

from Helper.VideoCapture import VideoCapture


class TestVideoCapture(unittest.TestCase):
    def test_typical_lifecycle(self):
        self.vidcap = VideoCapture(target="Assets/test_video.mp4")
        self.vidcap.start()
        time.sleep(0.1)
        frame = self.vidcap.get_current_frame()
        self.assertTrue(frame is not None)
        time.sleep(0.1)
        self.vidcap.stop()
        self.assertRaises(RuntimeError, self.vidcap.get_current_frame)

    def test_start(self):
        self.vidcap = VideoCapture(target="Assets/test_video.mp4")
        self.vidcap.start()
        frame = self.vidcap.get_current_frame()
        self.assertTrue(frame is not None)
        self.vidcap.stop()

    def test_stop(self):
        self.vidcap = VideoCapture(target="Assets/test_video.mp4")
        self.vidcap.start()
        self.vidcap.stop()
        self.assertRaises(RuntimeError, self.vidcap.get_current_frame)

    def test_start_twice(self):
        self.vidcap = VideoCapture(target="Assets/test_video.mp4")
        self.vidcap.start()
        self.assertRaises(RuntimeError, self.vidcap.start)
        self.vidcap.stop()

    def test_stop_twice(self):
        self.vidcap = VideoCapture(target="Assets/test_video.mp4")
        self.vidcap.start()
        self.vidcap.stop()
        self.assertRaises(RuntimeError, self.vidcap.stop)

    def test_multiple_instances(self):
        vidcap1 = VideoCapture(target="Assets/test_video.mp4")
        vidcap2 = VideoCapture(target="Assets/test_video.mp4")
        vidcap1.start()
        try:
            vidcap2.start()  # may or may not work depending on video capture target
        except RuntimeError:
            pass
        frame1 = vidcap1.get_current_frame()
        self.assertTrue(frame1 is not None)
        vidcap1.stop()
        vidcap2.start()
        try:
            vidcap1.start()  # may or may not work depending on video capture target
        except RuntimeError:
            pass
        frame2 = vidcap2.get_current_frame()
        self.assertTrue(frame2 is not None)
        vidcap2.stop()

    def test_take_photo(self):
        frame = VideoCapture.take_photo(target="Assets/test_video.mp4")
        self.assertTrue(frame is not None)

    def tearDown(self):  # this will get called after every test
        if hasattr(self, 'vidcap') and self.vidcap._video_capture_thread.is_alive():
            self.vidcap.stop()


if __name__ == '__main__':
    unittest.main()
