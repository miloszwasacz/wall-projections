from typing import List
import numpy as np
import cv2
from cv2 import aruco
# noinspection PyPackages
from . import importme, numpy_dotnet_converters as npnet
from VideoCaptureThread import VideoCaptureThread

DICT = aruco.getPredefinedDictionary(aruco.DICT_7X7_100)
"""The CV2 ArUco dictionary, which we detect for. Must match the one used in Internal/aruco_generator.
Defined here https://docs.opencv.org/3.4/d9/d6a/group__aruco.html#gac84398a9ed9dd01306592dd616c2c975."""

DISPLAY_RESULTS = False
"""Displays labled ArUco detection on a CV2 window, useful for debugging"""

class Calibrator:
    #Methods for generating transformation matrix

    @staticmethod
    def calibrate(projector_id_to_coord : dict[int, tuple[int, int]]): #TODO typing returns a clr system array
        photo = VideoCaptureThread.takePhoto()
        camera_id_to_coord = Calibrator._detectArUcos(photo)
        transformation_matrix = Calibrator._get_transformation_matrix(projector_id_to_coord, camera_id_to_coord)
        return npnet.asNetArray(transformation_matrix)

    @staticmethod
    def _detect_ArUcos(img: np.ndarray) -> dict[int, tuple[np.float32, np.float32]]:
        """
        Detects ArUcos in an image and returns a dictionary of ids to coordinates
        """
        corners, ids, _ = aruco.detectMarkers(img, DICT, parameters=aruco.DetectorParameters())

        if DISPLAY_RESULTS == True:
            cv2.imshow("Labled Aruco Markers", aruco.drawDetectedMarkers(img, corners, ids))
            cv2.waitKey(60000)

        detectedCoords: dict[int, tuple[np.float32, np.float32]] = {}
        if ids is None:
            return detectedCoords
        for i in range(ids.size):
            iD = ids[i][0]
            topLeftCorner = (corners[i][0][0][0], corners[i][0][0][1])
            detectedCoords[iD] = topLeftCorner
        return detectedCoords

    @staticmethod
    def _get_transformation_matrix(from_coords : dict[int, tuple[int, int]], to_coords : dict[int, tuple[np.float32, np.float32]]) -> np.ndarray:
        """
        Returns a transformation matrix from the coords stored in one dictionary to another
        """
        from_array = []
        to_array = []
        for iD in from_coords:
            if iD in to_coords: #if iD in both dictionaries
                from_array.append(from_coords[iD])
                to_array.append(to_coords[iD])

        if len(from_array) < 4:
            print(len(from_array), "matching coords found, at least 4 are required for calibration.")
            return np.identity(3)  #TODO return identity for now; replace with something more meaningful later

        from_np_array = np.array(from_array, dtype=np.float32)
        to_np_array = np.array(to_array, dtype=np.float32)

        return cv2.findHomography(from_np_array, to_np_array)[0]
    
    #Methods for doing transformations

    def __init__(self, transformation_matrix : np.ndarray, camera_res : tuple[int, int]):
        self._transformation_matrix = transformation_matrix
        self._inverse_transformation_matrix = np.linalg.inv(transformation_matrix)
        self._camera_res = camera_res
    
    def proj_to_cam(self, proj_coords : tuple[int, int]) -> tuple[np.float32, np.float32]:
        """
        Transforms a coordinate in projector space to a coordinate in camera space
        """
        rotated_vector = np.array(proj_coords, np.float32).reshape(-1, 1, 2)
        transformed_vector =  cv2.perspectiveTransform(rotated_vector, self._transformation_matrix)[0][0]
        return (transformed_vector[0], transformed_vector[1])

    def proj_to_norm(self, proj_coords : tuple[int, int]) -> tuple[float, float]:
        """
        Transforms a coordinate in projector space to a coordinate in camera space
        then normalizes them as floats between 0 and 1. (the coordinate system mediapipes uses)
        """
        camera_coords = self.proj_to_cam(proj_coords)
        return (camera_coords[0]/self._camera_res[0], camera_coords[1]/self._camera_res[1])

    def cam_to_proj(self, cam_coords : tuple[float, float]) -> tuple[int, int]:
        """
        Transforms a coordinate in camera space to a coordinate in projector space
        """
        rotated_vector = np.array(cam_coords, np.float32).reshape(-1, 1, 2)
        transformed_vector =  cv2.perspectiveTransform(rotated_vector, self._inverse_transformation_matrix)[0][0]
        return (int(transformed_vector[0]), int(transformed_vector[1]))
    
    def norm_to_proj(self, norm_coords : tuple[float, float]) -> tuple[int, int]:
        """
        Transforms a normalized coordinate in camera space to a coordinate in projector space
        """
        cam_coords = (norm_coords[0]*self._camera_res[0], norm_coords[1]*self._camera_res[1])
        return self.cam_to_proj(cam_coords)

def calibrate(projector_id_to_coord : dict[int, tuple[int, int]]): #TODO typing returns a clr system array
    return Calibrator.calibrate(projector_id_to_coord)