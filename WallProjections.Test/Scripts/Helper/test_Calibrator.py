import unittest
import numpy as np
import logging
import cv2
from cv2 import aruco
from typing import Union
from WallProjections.Scripts.Helper.Calibrator import Calibrator
logging.basicConfig(level=logging.INFO)

#HELPER FUNCTIONS:

TOLERANCE = 0.001

def as_numpy_dict(dictionary: Union[dict[int, tuple[int, int]], dict[int, tuple[float, float]]]) -> dict[int, tuple[np.float32, np.float32]]:
    return {key: (np.float32(value[0]), np.float32(value[1])) for key, value in dictionary.items()}
def matrix_same(a: np.ndarray, b: np.ndarray, tolerance: float =TOLERANCE) -> bool:
    # Returns true if matrix A and B are the same, within tolerance
    return np.allclose(a, b, atol=tolerance)
def dictionaries_the_same(dict_a : dict[int, tuple[np.float32, np.float32]], dict_b: dict[int, tuple[np.float32, np.float32]], tolerance : float = TOLERANCE) -> bool:
    if set(dict_a.keys()) != set(dict_b.keys()):
        logging.warning(f"Dictionaries do not share the same keys: dict_a {set(dict_a)} not equal to dict_b {set(dict_b)}")
        return False
    for key in dict_a:
        if not np.allclose(dict_a[key], dict_b[key], tolerance):
            logging.warning(f"Dictionaries at key {key} do not share the same value: dict_a's {dict_a[key]} not equal to dict_b's {dict_b[key]}")
            return False
    return True

#TESTDETECTARUCOS

ARUCO_GRID_DICT =     {0: (25, 25), 1: (175, 25), 2: (325, 25), 3: (475, 25),
    4: (625, 25), 5: (775, 25), 6: (925, 25), 7: (1075, 25),
    8: (1225, 25), 9: (1375, 25), 10: (1525, 25), 11: (1675, 25),
    12: (25, 175), 13: (175, 175), 14: (325, 175), 15: (475, 175)}
class TestDetectArUcos(unittest.TestCase):
    def test_detect_from_aruco_grid(self):
        """
        Test that we can detect all the aruco's on aruco_grid.jpg as represented in ARUCO_GRID_DICT
        """
        img = cv2.imread("../../Assets/Python/aruco_grid.jpg")
        detected_dict = Calibrator._detect_ArUcos(img)
        self.assertTrue(dictionaries_the_same(as_numpy_dict(ARUCO_GRID_DICT), detected_dict))

#TESTGETTRANSFORMATIONMATRIX

DICT = {
            1: (7.0, 1.0),
            2: (5.3, 7.9),
            3: (8.5, 7.9),
            4: (3.2, 2.2)
        }
class TestGetTransformationMatrix(unittest.TestCase):
    def test_identical_points_return_identity(self):
        """
        Test that passing in the same dictionary twice returns the identity matrix (no transformation)
        """
        t_matrix = Calibrator._get_transformation_matrix(as_numpy_dict(DICT), DICT)
        self.assertTrue(matrix_same(t_matrix, np.identity(3)))


#TEST

class TestTransformations(unittest.TestCase):

    def test_norm_to_proj_to_norm(self):
        """
        Transforming to projector space and back again should do nothing
        """
        calibrator = Calibrator(np.identity(3), (500, 300))
        abs_vector = (0.5, 0.5)
        transformed_vector = calibrator.proj_to_norm(calibrator.norm_to_proj(abs_vector))
        self.assertTrue(np.allclose(abs_vector, transformed_vector, atol=TOLERANCE))

if __name__ == '__main__':
    unittest.main()


