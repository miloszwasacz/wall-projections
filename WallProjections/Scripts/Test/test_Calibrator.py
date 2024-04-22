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


def gen_random_dict(size: int, mul: int = 50) -> dict[int, tuple[np.float32, np.float32]]:
    return {i: (np.float32(np.random.randn() * mul), np.float32(np.random.randn() * mul)) for i in range(size)}


def gen_random_t_matrix(mul: int = 10) -> np.ndarray:
    return (np.random.rand(3, 3) * mul).astype(np.float32)


def transform(vect: (np.float32, np.float32), t_matrix: np.ndarray) -> (np.float32, np.float32):
    rotated_vect = np.array(vect, np.float32).reshape(-1, 1, 2)
    transformed_vect = cv2.transform(rotated_vect, t_matrix)[0][0]
    return np.float32(transformed_vect[0]), np.float32(transformed_vect[1])


def as_numpy_dict(dictionary: Union[dict[int, tuple[int, int]], dict[int, tuple[float, float]]]) -> dict[
    int, tuple[np.float32, np.float32]]:
    return {key: (np.float32(value[0]), np.float32(value[1])) for key, value in dictionary.items()}


def as_python_dict(dictionary: Union[dict[int, tuple[np.float32, np.float32]], dict[int, tuple[float, float]]]) -> dict[
    int, tuple[float, float]]:
    return {key: (float(value[0]), float(value[1])) for key, value in dictionary.items()}


def matrix_equivalent(a: np.ndarray, b: np.ndarray, tolerance: float = TOLERANCE) -> bool:
    """Transforms a number of random vectors by both matrix a and b. Checks transformed vectors are equivalent"""
    point_dict = gen_random_dict(10)
    for point in point_dict.values():
        transformed_point_a = transform(point, a)
        transformed_point_b = transform(point, b)
        if not np.allclose(transformed_point_a, transformed_point_b, tolerance):
            logging.error(f"{a} not equivalent to {b}")
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


REPEAT_COUNT = 5


class TestGetTransformationMatrix(unittest.TestCase):
    def test_identical_matrix_return_identity(self):
        for _ in range(REPEAT_COUNT):
            points = gen_random_dict(50)
            t_matrix = Calibrator._get_transformation_matrix(points, as_python_dict(points))
            self.assertTrue(matrix_equivalent(t_matrix, np.identity(3)))

    def test_recalculate_matrix(self):
        for _ in range(REPEAT_COUNT):
            points = gen_random_dict(50)
            t_matrix = gen_random_t_matrix()
            transformed_points = {key: transform(point, t_matrix) for key, point in points.items()}
            recalculated_matrix = Calibrator._get_transformation_matrix(points, as_python_dict(transformed_points))
            self.assertTrue(matrix_equivalent(t_matrix, recalculated_matrix))
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
