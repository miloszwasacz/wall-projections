# This is a mock module that is used in unit tests but is replaced by the actual platform-specific module in production.


import numpy as np
from Scripts.Helper.VideoCapture import VideoCapture


def getVideoCapture(video_capture_target: int | str) -> VideoCapture:
    """
    Create a new VideoCapture that uses the specified camera index.
    """
    return VideoCapture(target=video_capture_target)


def take_photo(video_capture_target: int | str) -> np.ndarray:
    """
    Take a photo using the specified camera index.
    """
    return VideoCapture.take_photo(target=video_capture_target)
