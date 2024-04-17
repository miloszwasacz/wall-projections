import logging
import unittest
from typing import Union

import cv2
import numpy as np

# noinspection PyPackages
from Helper.Calibrator import Calibrator

logging.basicConfig(level=logging.INFO)

# HELPER FUNCTIONS:

TOLERANCE = 0.001


def as_numpy_dict(dictionary: Union[dict[int, tuple[int, int]], dict[int, tuple[float, float]]]) -> dict[
    int, tuple[np.float32, np.float32]]:
    return {key: (np.float32(value[0]), np.float32(value[1])) for key, value in dictionary.items()}


def matrix_equivalent(a: np.ndarray, b: np.ndarray, tolerance: float = TOLERANCE) -> bool:
    """Transforms a number of random vectors by both matrix a and b. Checks transformed vectors are equivallent"""
    vector_count = 10
    for _ in range(vector_count):
        random_vector = np.random.rand(2)
        rotated_vector = np.array(random_vector, np.float32).reshape(-1, 1, 2)
        transformed_vector_a = cv2.transform(rotated_vector, a)[0][0]
        transformed_vector_b = cv2.transform(rotated_vector, b)[0][0]
        if not np.allclose(transformed_vector_a, transformed_vector_b, tolerance):
            return False
    return True


def dictionaries_the_same(dict_a: dict[int, tuple[np.float32, np.float32]],
                          dict_b: dict[int, tuple[np.float32, np.float32]], tolerance: float = TOLERANCE) -> bool:
    if set(dict_a.keys()) != set(dict_b.keys()):
        logging.warning(
            f"Dictionaries do not share the same keys: dict_a {set(dict_a)} not equal to dict_b {set(dict_b)}")
        return False
    for key in dict_a:
        if not np.allclose(dict_a[key], dict_b[key], tolerance):
            logging.warning(
                f"Dictionaries at key {key} do not share the same value: dict_a's {dict_a[key]} not equal to dict_b's {dict_b[key]}")
            return False
    return True


# TESTDETECTARUCOS

ARUCO_GRID_DICT = {0: (25, 25), 1: (175, 25), 2: (325, 25), 3: (475, 25),
                   4: (625, 25), 5: (775, 25), 6: (925, 25), 7: (1075, 25),
                   8: (1225, 25), 9: (1375, 25), 10: (1525, 25), 11: (1675, 25),
                   12: (25, 175), 13: (175, 175), 14: (325, 175), 15: (475, 175)}


class TestDetectArUcos(unittest.TestCase):
    def test_detect_from_aruco_grid(self):
        print("temp")
        self.assertTrue(True)


# TESTGETTRANSFORMATIONMATRIX

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
        self.assertTrue(matrix_equivalent(t_matrix, np.identity(3)))


# TEST

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
