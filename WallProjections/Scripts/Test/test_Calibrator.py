import logging
import unittest
from typing import Union

import cv2
import numpy as np
from cv2 import aruco

from Scripts.Helper.Calibrator import Calibrator
from Scripts.Test.helper import get_asset

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

ARUCO_POSITION_DICT = {0: (25, 25), 1: (175, 25), 2: (325, 25), 3: (475, 25),
                       4: (625, 25), 5: (775, 25), 6: (925, 25), 7: (1075, 25),
                       8: (1225, 25), 9: (1375, 25), 10: (1525, 25), 11: (1675, 25),
                       12: (25, 175), 13: (175, 175), 14: (325, 175), 15: (475, 175)}
ARUCO_DICT = aruco.getPredefinedDictionary(aruco.DICT_7X7_100)


class TestDetectArUcos(unittest.TestCase):
    def test_detect_generated_grid(self):
        image = np.full((1080, 1920), 255, np.uint8)  # generate empty background
        for code, top_left_corner in ARUCO_POSITION_DICT.items():
            aruco_image = aruco.generateImageMarker(ARUCO_DICT, code, 100, borderBits=1)  # fetch ArUco
            # replace pixels in image with the ArUco's pixels
            image[top_left_corner[1]:top_left_corner[1] + aruco_image.shape[0],
                  top_left_corner[0]:top_left_corner[0] + aruco_image.shape[1]] = aruco_image

        detected_position_dict = Calibrator._detect_ArUcos(image)
        self.assertTrue(dictionaries_the_same(detected_position_dict, ARUCO_POSITION_DICT))

    def test_detect_known_images(self):
        known_dict = {0: (177.0, 248.0), 1: (592.0, 279.0), 2: (980.0, 315.0), 3: (1342.0, 350.0), 4: (1681.0, 381.0), 5: (1998.0, 411.0), 6: (2294.0, 439.0), 7: (2573.0, 464.0), 8: (2835.0, 489.0), 9: (3082.0, 511.0), 10: (3316.0, 532.0), 11: (3536.0, 554.0), 12: (185.0, 631.0), 13: (599.0, 654.0), 14: (988.0, 676.0), 15: (1348.0, 699.0), 16: (1686.0, 720.0), 17: (2000.0, 739.0), 18: (2296.0, 756.0), 19: (2573.0, 771.0), 20: (2834.0, 785.0), 21: (3081.0, 799.0), 22: (3314.0, 813.0), 23: (3536.0, 825.0), 24: (193.0, 1016.0), 25: (608.0, 1025.0), 26: (995.0, 1034.0), 27: (1356.0, 1045.0), 28: (1693.0, 1055.0), 29: (2004.0, 1063.0), 30: (2297.0, 1070.0), 31: (2573.0, 1075.0), 32: (2834.0, 1080.0), 33: (3081.0, 1085.0), 34: (3314.0, 1090.0), 35: (3533.0, 1096.0), 36: (204.0, 1399.0), 37: (615.0, 1394.0), 38: (1003.0, 1390.0), 39: (1364.0, 1387.0), 40: (1698.0, 1385.0), 41: (2008.0, 1383.0), 42: (2299.0, 1379.0), 43: (2573.0, 1376.0), 44: (2834.0, 1372.0), 45: (3081.0, 1370.0), 46: (3313.0, 1367.0), 47: (3533.0, 1365.0), 48: (215.0, 1780.0), 49: (627.0, 1762.0), 50: (1011.0, 1744.0), 51: (1370.0, 1727.0), 52: (1703.0, 1712.0), 53: (2012.0, 1697.0), 54: (2302.0, 1685.0), 55: (2576.0, 1673.0), 56: (2836.0, 1663.0), 57: (3082.0, 1652.0), 58: (3314.0, 1642.0), 59: (3533.0, 1632.0), 60: (228.0, 2158.0), 61: (637.0, 2126.0), 62: (1019.0, 2096.0), 63: (1376.0, 2067.0), 64: (1708.0, 2039.0), 65: (2017.0, 2014.0), 66: (2306.0, 1991.0), 67: (2580.0, 1971.0), 68: (2839.0, 1952.0), 69: (3084.0, 1933.0), 70: (3315.0, 1916.0), 71: (3534.0, 1899.0)}
        image = cv2.imread(get_asset("arucos_example.jpg"))
        detected_dict = Calibrator._detect_ArUcos(image)
        self.assertTrue(dictionaries_the_same(known_dict, detected_dict))

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


if __name__ == '__main__':
    unittest.main()
