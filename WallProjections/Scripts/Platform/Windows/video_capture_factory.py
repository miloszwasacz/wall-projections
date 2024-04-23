﻿import cv2
import numpy as np
# noinspection PyPackages
from .Helper.VideoCapture import VideoCapture


def getVideoCapture(camera_index: int) -> VideoCapture:
    """
    Create a new VideoCapture that uses the specified camera index.
    """
    return VideoCapture(target=camera_index, backend=cv2.CAP_DSHOW)


def take_photo(camera_index: int) -> np.ndarray:
    """
    Take a photo using the specified camera index.
    """
    return VideoCapture.take_photo(target=camera_index, backend=cv2.CAP_DSHOW)