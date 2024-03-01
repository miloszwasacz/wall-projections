from typing import List
import numpy as np
# noinspection PyPackages
from . import importme, numpy_dotnet_converters as npnet


def calibrate(aruco_dict) -> np.ndarray:
    """
    Takes a dictionary of id's to coords
    returns a 3x3 np array of 32 bit numpy floats
    """

    return npnet.asNetArray(np.array(calibrate2(aruco_dict), dtype=np.float32))


def calibrate2(aruco_dict) -> List[List[float]]:
    """
    Takes a dictionary of id's to coords
    returns a 3x3 List of 64 bit python standard floats

    """
    importme.import_me()  # test importing works

    return [[0.0, 0.1, 0.2], [1.0, 1.1, 1.2], [2.0, 2.1, 2.2]]
