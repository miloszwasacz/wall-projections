import logging
import numpy as np
import cv2
from cv2 import aruco
# noinspection PyPackages
from .VideoCaptureThread import VideoCaptureThread

DICT = aruco.getPredefinedDictionary(aruco.DICT_7X7_100)
"""The CV2 ArUco dictionary, which we detect for. Must match the one used in Internal/aruco_generator.
Defined here https://docs.opencv.org/3.4/d9/d6a/group__aruco.html#gac84398a9ed9dd01306592dd616c2c975."""

DISPLAY_RESULTS = False
"""Displays labeled ArUco detection on a CV2 window, useful for debugging"""


class Calibrator:
    # Methods for generating transformation matrix

    @staticmethod
    def calibrate(projector_id_to_coord: dict[int, tuple[float, float]]) -> np.ndarray:  # returns a 3x3 array of 32bit floats

        photo = VideoCaptureThread.take_photo()
        camera_id_to_coord = Calibrator._detect_ArUcos(photo)
        return Calibrator._get_transformation_matrix(camera_id_to_coord, projector_id_to_coord)

    @staticmethod
    def _detect_ArUcos(img: np.ndarray) -> dict[int, tuple[np.float32, np.float32]]:
        """
        Detects ArUcos in an image and returns a dictionary of ids to coordinates
        """
        corners, ids, _ = aruco.detectMarkers(img, DICT, parameters=aruco.DetectorParameters())

        if DISPLAY_RESULTS:
            cv2.imshow("Labeled Aruco Markers", aruco.drawDetectedMarkers(img, corners, ids))
            cv2.waitKey(60000)

        detected_coords: dict[int, tuple[np.float32, np.float32]] = {}
        if ids is None:
            return detected_coords
        for i in range(ids.size):
            aruco_id = int(ids[i][0])
            top_left_corner = (corners[i][0][0][0], corners[i][0][0][1])
            detected_coords[aruco_id] = top_left_corner
        return detected_coords

    @staticmethod
    def _get_transformation_matrix(
            from_coords: dict[int, tuple[np.float32, np.float32]],
            to_coords: dict[int, tuple[float, float]]
    ) -> np.ndarray:  # returns a 3x3 array of 32bit floats
        """
        Returns a transformation matrix from the coords stored in one dictionary to another
        """
        from_array = []
        to_array = []
        logging.info("From Coords " + str(from_coords))
        logging.info("To Coords " + str(to_coords))
        for iD in from_coords:
            if iD in to_coords:  # if iD in both dictionaries
                logging.info("Matching coords found: " + str(iD) + " " + str(from_coords[iD]) + " " + str(to_coords[iD]))
                from_array.append(from_coords[iD])
                to_array.append(to_coords[iD])

        if len(from_array) < 4:
            logging.info(str(len(from_array)) + " matching coords found, at least 4 are required for calibration.")
            return np.identity(3)  # TODO return identity for now; replace with something more meaningful later

        from_np_array = np.array(from_array, dtype=np.float32)
        to_np_array = np.array(to_array, dtype=np.float32)

        return cv2.findHomography(from_np_array, to_np_array)[0]

    # Methods for doing transformations

    def __init__(self, transformation_matrix: np.ndarray, camera_res: tuple[int, int]):
        self._transformation_matrix = transformation_matrix
        self._inverse_transformation_matrix = np.linalg.inv(transformation_matrix)
        self._camera_res = camera_res


    def proj_to_cam(self, proj_coords: tuple[int, int]) -> tuple[np.float32, np.float32]:
        """
        Transforms a coordinate in projector space to a coordinate in camera space
        """
        rotated_vector = np.array(proj_coords, np.float32).reshape(-1, 1, 2)
        transformed_vector = cv2.perspectiveTransform(rotated_vector, self._transformation_matrix)[0][0]
        return transformed_vector[0], transformed_vector[1]

    def proj_to_norm(self, proj_coords: tuple[int, int]) -> tuple[float, float]:
        """
        Transforms a coordinate in projector space to a coordinate in camera space
        then normalizes them as floats between 0 and 1. (the coordinate system mediapipe uses)
        """
        camera_coords = self.proj_to_cam(proj_coords)
        return camera_coords[0] / self._camera_res[0], camera_coords[1] / self._camera_res[1]

    def cam_to_proj(self, cam_coords: tuple[float, float]) -> tuple[int, int]:
        """
        Transforms a coordinate in camera space to a coordinate in projector space
        """
        rotated_vector = np.array(cam_coords, np.float32).reshape(-1, 1, 2)
        transformed_vector = cv2.perspectiveTransform(rotated_vector, self._inverse_transformation_matrix)[0][0]
        return int(transformed_vector[0]), int(transformed_vector[1])

    def norm_to_proj(self, norm_coords: tuple[float, float]) -> tuple[int, int]:
        """
        Transforms a normalized coordinate in camera space to a coordinate in projector space
        """
        cam_coords = (norm_coords[0] * self._camera_res[0], norm_coords[1] * self._camera_res[1])
        return self.cam_to_proj(cam_coords)

    def scaler_to_norm(self, scaler : float) -> float:
        det = np.linalg.det(self._transformation_matrix)
        return scaler*det

