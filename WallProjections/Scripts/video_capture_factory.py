# This is a mock module that is used in unit tests but is replaced by the actual platform-specific module in production.


import numpy as np
from Scripts.Helper.VideoCapture import VideoCapture


def getVideoCapture(camera_index: int) -> VideoCapture:
    """
    Create a new VideoCapture that uses the specified camera index.
    """
    return VideoCapture(target=camera_index)


def take_photo(camera_index: int) -> np.ndarray:
    """
    Take a photo using the specified camera index.
    """
    return VideoCapture.take_photo(target=camera_index)
