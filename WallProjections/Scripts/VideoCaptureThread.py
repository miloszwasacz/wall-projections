import threading
import numpy as np
import cv2

VIDEO_CAPTURE_TARGET: int | str = 0
"""Camera ID, video filename, image sequence filename or video stream URL to capture video from.

Set to 0 to use default camera.

See https://docs.opencv.org/3.4/d8/dfe/classcv_1_1VideoCapture.html#a949d90b766ba42a6a93fe23a67785951 for details."""

VIDEO_CAPTURE_BACKEND: int = cv2.CAP_ANY
"""The API backend to use for capturing video. 

Use ``cv2.CAP_ANY`` to auto-detect. 

See https://docs.opencv.org/3.4/d4/d15/group__videoio__flags__base.html for more options."""

VIDEO_CAPTURE_PROPERTIES: dict[int, int] = {cv2.CAP_PROP_FPS: 30}
"""Extra properties to use for OpenCV video capture.

Tweaking these properties could be found useful in case of any issues with capturing video.

See https://docs.opencv.org/3.4/d4/d15/group__videoio__flags__base.html#gaeb8dd9c89c10a5c63c139bf7c4f5704d and
https://docs.opencv.org/3.4/dc/dfc/group__videoio__flags__others.html."""



class VideoCaptureThread(threading.Thread):
    def __init__(self):
        super().__init__()
        self.current_frame = None
        self.stopping = False

    def run(self):
        logging.info("Initialising video capture...")
        video_capture = cv2.VideoCapture()
        success = video_capture.open(VIDEO_CAPTURE_TARGET, VIDEO_CAPTURE_BACKEND)
        if not success:
            raise RuntimeError("Error opening video capture - perhaps the video capture target or backend is invalid.")
        for prop_id, prop_value in VIDEO_CAPTURE_PROPERTIES.items():
            supported = video_capture.set(prop_id, prop_value)
            if not supported:
                logging.warning(f"Property id {prop_id} is not supported by video capture backend {VIDEO_CAPTURE_BACKEND}.")

        while video_capture.isOpened():
            success, video_capture_img = video_capture.read()
            if not success:
                logging.warning("Unsuccessful video read; ignoring frame.")
                continue

            video_capture_img = cv2.cvtColor(video_capture_img, cv2.COLOR_BGR2RGB)  # convert to RGB
            new_dim = (int(video_capture_img.shape[1] / video_capture_img.shape[0] * 480), 480)
            video_capture_img = cv2.resize(video_capture_img, new_dim, interpolation=cv2.INTER_NEAREST)  # normalise size
            self.current_frame = video_capture_img

            if self.stopping:
                logging.info("Stopping video capture.")
                break

        video_capture.release()

    def stop(self):
        """
        Stop the video capture after the current frame.

        Call `Thread.join` after this to block until the thread has stopped.
        """
        self.stopping = True


def takePhoto() -> np.ndarray:
    """
    Returns a photo from a detectable camera
    """

    videoCaptureThread = VideoCaptureThread()
    videoCaptureThread.start()
    image = None
    while image is None:
        image = videoCaptureThread.current_frame
        cv2.waitKey(500)
    videoCaptureThread.stop()
    videoCaptureThread.join()

    return image
