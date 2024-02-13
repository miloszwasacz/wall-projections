import logging
from calibrate import *
import numpy as np
import cv2
from cv2 import aruco
import mediapipe as mp
from typing import Dict, Tuple

MAX_NUM_HANDS: int = 4
"""The maximum number of hands to detect."""

MIN_DETECTION_CONFIDENCE: float = 0.5
"""The minimum confidence for hand detection to be considered successful.

Must be between 0 and 1.

See https://developers.google.com/mediapipe/solutions/vision/hand_landmarker."""

MIN_TRACKING_CONFIDENCE: float = 0.5
"""The minimum confidence for hand detection to be considered successful.

Must be between 0 and 1.

See https://developers.google.com/mediapipe/solutions/vision/hand_landmarker."""


VIDEO_CAPTURE_TARGET: int | str = 0
"""Camera ID, video filename, image sequence filename or video stream URL to capture video from.

Set to 0 to use default camera.

See https://docs.opencv.org/3.4/d8/dfe/classcv_1_1VideoCapture.html#a949d90b766ba42a6a93fe23a67785951 for details."""

VIDEO_CAPTURE_BACKEND: int = cv2.CAP_ANY
"""The API backend to use for capturing video. 

Use ``cv2.CAP_ANY`` to auto-detect. 

See https://docs.opencv.org/3.4/d4/d15/group__videoio__flags__base.html for more options."""

VIDEO_CAPTURE_PROPERTIES: dict[int, int] = {}
"""Extra properties to use for OpenCV video capture.

Tweaking these properties could be found useful in case of any issues with capturing video.

See https://docs.opencv.org/3.4/d4/d15/group__videoio__flags__base.html#gaeb8dd9c89c10a5c63c139bf7c4f5704d and
https://docs.opencv.org/3.4/dc/dfc/group__videoio__flags__others.html."""


def normalizeToCamera(coord : Tuple[float, float], width : int, height : int) -> Tuple[float, float]:
    """
    Takes in a coordinate returned by mediapipes given as two values between 0 and 1 for x and y
    and returns the corresponding coordinate on the camera. 
    """
    return (coord.x*width, coord.y*height)


def drawCirlces(tMatrix : np.ndarray) -> None:
    """
    draws circles on index fingers based on transformtion matrix
    """

    # initialise ML hand-tracking model
    logging.info("Initialising hand-tracking model...")
    hands_model = mp.solutions.hands.Hands(max_num_hands=MAX_NUM_HANDS,
                                           min_detection_confidence=MIN_DETECTION_CONFIDENCE,
                                           min_tracking_confidence=MIN_TRACKING_CONFIDENCE)

    logging.info("Initialising video capture...")
    video_capture = cv2.VideoCapture()
    success = video_capture.open(VIDEO_CAPTURE_TARGET, VIDEO_CAPTURE_BACKEND)
    if not success:
        raise RuntimeError("Error opening video capture - perhaps the video capture target or backend is invalid.")
    for prop_id, prop_value in VIDEO_CAPTURE_PROPERTIES.items():
        supported = video_capture.set(prop_id, prop_value)
        if not supported:
            logging.warning(f"Property id {prop_id} is not supported by video capture backend {VIDEO_CAPTURE_BACKEND}.")


    logging.info("Initialisation done.")

    # basic opencv + mediapipe stuff from https://www.youtube.com/watch?v=v-ebX04SNYM

    while video_capture.isOpened():
        success, video_capture_img = video_capture.read()
        if not success:
            logging.warning("Unsuccessful video read; ignoring frame.")
            continue

        camera_height, camera_width, _ = video_capture_img.shape

        # run model
        video_capture_img_rgb = cv2.cvtColor(video_capture_img, cv2.COLOR_BGR2RGB)  # convert to RGB
        model_output = hands_model.process(video_capture_img_rgb)

        image=np.full((1080,1920), 0,np.uint8) #generate empty background
        if hasattr(model_output, "multi_hand_landmarks") and model_output.multi_hand_landmarks is not None:
            #get index finger coords in camera space
            index_coords = [normalizeToCamera(landmarks.landmark[8], camera_width, camera_height) for landmarks in
                                model_output.multi_hand_landmarks]
            #transform coords with matrix and draw circle
            for index_coord in index_coords:
                index_coord = transform(index_coord, tMatrix)
                index_coord = (int(index_coord[0]), int(index_coord[1]))
                cv2.circle(image, index_coord, 3, 255, 2)

        cv2.imshow("Calibration", image)

        # development key inputs
        key = chr(cv2.waitKey(1) & 0xFF).lower()
        if key == "q":
            break

    # clean up
    video_capture.release()
    cv2.destroyAllWindows()
    hands_model.close()



if __name__ == "__main__":
    tMatrix = projectDetectGetTransform()
    if not tMatrix is None:
        drawCirlces(tMatrix)