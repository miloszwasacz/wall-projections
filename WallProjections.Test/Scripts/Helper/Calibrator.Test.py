import unittest
import numpy as np
import cv2
from cv2 import aruco
from WallProjections.Scripts.Helper.Calibrator import Calibrator

TOLERANCE = 0.001
DICT1 = {
            1: (7.0, 1.0),
            2: (5.3, 7.9),
            3: (8.5, 7.9),
            4: (3.2, 2.2)
        }


def as_numpy_dict(dictionary: dict[int, tuple[float, float]]) -> dict[int, tuple[np.float32, np.float32]]:
    return {key: (np.float32(value[0]), np.float32(value[1])) for key, value in dictionary.items()}


def matrix_same(a: np.ndarray, b: np.ndarray, tolerance=TOLERANCE) -> bool:
    # Returns true if matrix A and B are the same, within tolerance
    return np.allclose(a, b, atol=tolerance)


def is_matrix_identity(a: np.ndarray, tolerance=TOLERANCE) -> bool:
    # Returns true if matrix A is the identity matrix, within tolerance.
    return matrix_same(a, np.identity(3))


class GetTransformTests(unittest.TestCase):
    def identical_points_return_identity(self):
        t_matrix = Calibrator._get_transformation_matrix(as_numpy_dict(DICT1), DICT1)
        self.assertTrue(is_matrix_identity(t_matrix))


if __name__ == '__main__':
    unittest.main()
