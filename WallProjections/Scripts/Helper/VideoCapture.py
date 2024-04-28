import threading

import cv2
import numpy as np
import time

# noinspection PyPackages
from .logger import get_logger

logger =get_logger()

DEFAULT_TARGET: int | str = 0
"""Camera ID, video filename, image sequence filename or video stream URL to capture video from.

Set to 0 to auto-detect.

See https://docs.opencv.org/3.4/d8/dfe/classcv_1_1VideoCapture.html#a949d90b766ba42a6a93fe23a67785951 for details."""

DEFAULT_BACKEND: int = cv2.CAP_ANY
"""The API backend to use for capturing video. 

Use ``cv2.CAP_ANY`` to auto-detect. 

See https://docs.opencv.org/3.4/d4/d15/group__videoio__flags__base.html for more options."""

DEFAULT_PROPERTIES: dict[int, int] = {cv2.CAP_PROP_FPS: 30}
"""Extra properties to use for OpenCV video capture.

Tweaking these properties could be found useful in case of any issues with capturing video.

See https://docs.opencv.org/3.4/d4/d15/group__videoio__flags__base.html#gaeb8dd9c89c10a5c63c139bf7c4f5704d and
https://docs.opencv.org/3.4/dc/dfc/group__videoio__flags__others.html."""

RECORD_VIDEO: bool = False


class VideoCapture:
    """Helper class for capturing video (or a photo)."""

    def __init__(self, target: int | str | None = None, backend: int | None = None,
                 properties: dict[int, int] | None = None) -> None:
        """Pass `None` for any parameter to choose a default value.

        :param target Camera ID, video filename, image sequence filename or video stream URL to capture video from.
        :param backend The API backend to use for capturing video.
        :param properties Extra properties to use for OpenCV video capture."""

        self.target: int | str = target or DEFAULT_TARGET
        self.backend: int = backend or DEFAULT_BACKEND
        self.properties: dict[int, int] = properties or DEFAULT_PROPERTIES
        self._video_capture_thread: threading.Thread | None = None
        self._current_frame: np.ndarray | None = None
        """The current frame in BGR."""
        self._stopping: bool = False
        self._lock: threading.Lock = threading.Lock()
        if RECORD_VIDEO:
            codec = cv2.VideoWriter.fourcc(*'DIVX')
            self._video_writer = cv2.VideoWriter('output.avi', codec, 30.0, (640, 480))

    def start(self) -> None:
        """Start capturing video. Blocks for a few seconds until the webcam is opened."""

        if self._video_capture_thread is not None:
            raise RuntimeError("Error starting video capture: Video capture is already running.")

        logger.info("Starting video capture...")
        self._video_capture_thread = threading.Thread(target=self._thread)
        self._video_capture_thread.start()

        # wait until camera is opened (1 min timeout)
        i = 0
        while self._current_frame is None and i < 600:
            time.sleep(0.1)
            i += 1

        if self._current_frame is None:
            self._stopping = True
            self._video_capture_thread.join()
            self._video_capture_thread = None
            self._current_frame = None
            raise RuntimeError("Error starting video capture: Camera failed to open (timed out).")

        time.sleep(1)  # wait for a bit more as it returns garbage at first

        logger.info("Video capture started.")

    def _thread(self) -> None:
        video_capture = cv2.VideoCapture()
        success = video_capture.open(self.target, self.backend)
        if not success:
            raise RuntimeError("Error opening video capture - perhaps the video capture target or backend is invalid, "
                               "or the camera is already in use.")
        for prop_id, prop_value in self.properties.items():
            supported = video_capture.set(prop_id, prop_value)
            if not supported:
                logger.warning(f"Property id {prop_id} is not supported by video capture backend {self.backend}.")

        while video_capture.isOpened():
            success, video_capture_img = video_capture.read()
            if success:
                # normalise and downscale resolution
                new_dim = (int(video_capture_img.shape[1] / video_capture_img.shape[0] * 480), 480)
                video_capture_img = cv2.resize(video_capture_img, new_dim, interpolation=cv2.INTER_NEAREST)
                self._current_frame = video_capture_img
                if RECORD_VIDEO:
                    self._video_writer.write(video_capture_img)
            else:
                logger.warning("Unsuccessful video read; ignoring frame.")

            if self._stopping:
                break

            # cap framerate if it's a test video
            if isinstance(self.target, str) and len(self.target) >= 4 and self.target[-4:] in (".mp4", ".avi"):
                time.sleep(1/30)

        video_capture.release()
        if RECORD_VIDEO:
            self._video_writer.release()

    def get_current_frame(self) -> np.ndarray:
        """Get the current frame in BGR. Throws a RuntimeError if the video capture is not running. (Or if video capture
        returns None for some reason.)"""

        with self._lock:
            if self._current_frame is None:
                raise RuntimeError("Error getting current frame: Video capture is not running. (Or video capture "
                                   "returned None for some reason.)")
            else:
                current_frame = self._current_frame.copy()

        return current_frame

    def stop(self) -> None:
        """Stop the video capture. Blocks for a few seconds until the webcam is closed."""

        if self._video_capture_thread is None:
            raise RuntimeError("Error stopping video capture: Video capture is not running.")

        logger.info("Stopping video capture...")
        self._stopping = True
        self._video_capture_thread.join()
        self._video_capture_thread = None
        self._current_frame = None
        logger.info("Video capture stopped.")

    @staticmethod
    def take_photo(target: int | str | None = None, backend: int | None = None,
                   properties: dict[int, int] | None = None) -> np.ndarray:
        """Returns a photo from a detectable camera."""
        vid_cap = VideoCapture(target, backend, properties)
        vid_cap.start()
        image = vid_cap.get_current_frame()
        vid_cap.stop()
        return image
