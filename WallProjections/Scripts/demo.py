from calibrate import *
from main import *
import numpy as np
import cv2
from cv2 import aruco
from typing import Dict, Tuple


def normalizeToPixel(coord, width, height):
    return (coord.x*width, coord.y*height)


def run2(tMatrix) -> None:  # This function is called by Program.cs
    """
    Captures video and runs the hand-detection model to handle the hotspots.

    :param event_listener: Callbacks for communicating events back to the C# side.
    """

    global hotspots

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
            # update hotspots
            index_coords = [normalizeToPixel(landmarks.landmark[8], camera_width, camera_height) for landmarks in
                                model_output.multi_hand_landmarks]
            
            for index_coord in index_coords:
                index_coord = transform(index_coord, tMatrix)
                index_coord = (int(index_coord[1]), int(index_coord[0]))
                cv2.circle(image, index_coord, 3, 255, 2)

        cv2.imshow("Calibration", image)

        # development key inputs
        key = chr(cv2.waitKey(1) & 0xFF).lower()
        if key == "c":
            media_finished()
        elif key == "q":
            break

    # clean up
    video_capture.release()
    cv2.destroyAllWindows()
    hands_model.close()



if __name__ == "__main__":
    tMatrix = example()
    run2(tMatrix)
