import time
import unittest

from Helper.VideoCapture import VideoCapture


class TestVideoCapture(unittest.TestCase):
    def assertVidCapStopped(self, vidcap):
        self.assertTrue(vidcap is None or
                        vidcap._video_capture_thread is None or
                        not vidcap._video_capture_thread.is_alive())

    def test_typical_lifecycle(self):
        self.vidcap = VideoCapture(target="Assets/test_video.mp4")
        self.vidcap.start()
        time.sleep(0.1)
        frame = self.vidcap.get_current_frame()
        self.assertTrue(frame is not None)
        time.sleep(0.1)
        self.vidcap.stop()
        self.assertRaises(RuntimeError, self.vidcap.get_current_frame)
        self.assertVidCapStopped(self.vidcap)

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
        self.assertVidCapStopped(self.vidcap)

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
        self.assertVidCapStopped(self.vidcap)

    def test_multiple_instances(self):
        vidcap1 = VideoCapture(target="Assets/test_video.mp4")
        vidcap2 = VideoCapture(target="Assets/test_video.mp4")
        vidcap1.start()
        vidcap2.start()  # should work for a video file but not for a webcam live feed
        frame1 = vidcap1.get_current_frame()
        self.assertTrue(frame1 is not None)
        frame2 = vidcap2.get_current_frame()
        self.assertTrue(frame2 is not None)
        vidcap1.stop()
        vidcap2.stop()
        self.assertVidCapStopped(vidcap1)
        self.assertVidCapStopped(vidcap2)

    def test_take_photo(self):
        frame = VideoCapture.take_photo(target="Assets/test_video.mp4")
        self.assertTrue(frame is not None)

    def tearDown(self):  # this will get called after every test
        # if hasattr(self, 'vidcap') and self.vidcap._video_capture_thread.is_alive():
        #     self.vidcap.stop()
        pass


if __name__ == '__main__':
    unittest.main()
