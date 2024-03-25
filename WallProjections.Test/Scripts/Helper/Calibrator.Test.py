import unittest
import numpy as np
from WallProjections.Scripts.Helper.Calibrator import Calibrator

TOLERANCE = 0.001


def matrix_same(a: np.ndarray, b: np.ndarray, tolerance=TOLERANCE) -> bool:
    # Returns true if matrix A and B are the same, within tolerance
    return np.allclose(a, b, atol=tolerance)


def matrix_identity(a: np.ndarray, tolerance=TOLERANCE) -> bool:
    # Returns true if matrix A is the identity matrix, within tolerance.
    return matrix_same(a, np.identity(3))
class MyTestCase(unittest.TestCase):
    def identical_points_return_identity(self):
        test_dict1 = {
            1: (1.0, 1.0),
            2: (3.0, 3.9),
            3: (131.1, 62.1),
            4: (11.1, 89.1)
        }
        test_dict2 = {key: (np.float32(value[0]), np.float32(value[1])) for key, value in test_dict1.items()}
        t_matrix = Calibrator._get_transformation_matrix(test_dict2, test_dict1)
        self.assertTrue(matrix_identity(t_matrix))


if __name__ == '__main__':
    unittest.main()
