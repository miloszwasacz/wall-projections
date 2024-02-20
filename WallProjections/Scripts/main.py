import logging
from typing import Tuple, List

import cv2
import mediapipe as mp
import numpy as np

from Calibrator import Calibrator
from EventListener import EventListener
from Hotspot import Hotspot

logging.basicConfig(level=logging.INFO)


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

FINGERTIP_INDICES: tuple[int, ...] = (4, 8, 12, 16, 20)
"""The indices for the thumb fingertip, index fingertip, ring fingertip, etc. 

This shouldn't need to be changed unless there's a breaking change upstream in mediapipe."""


hotspots: list[Hotspot] = []
"""The global list of hotspots."""


def media_finished() -> None:
    for hotspot in hotspots:
        hotspot.deactivate()


def run2(screenDimensions :Tuple[int, int], hotspots : List[Hotspot], calibrator) -> None:  # This function is called by Program.cs
    """
    Captures video and runs the hand-detection model to handle the hotspots.

    :param event_listener: Callbacks for communicating events back to the C# side.
    """



    # initialise ML hand-tracking model
    logging.info("Initialising hand-tracking model...")
    hands_model = mp.solutions.hands.Hands(max_num_hands=MAX_NUM_HANDS,
                                           min_detection_confidence=MIN_DETECTION_CONFIDENCE,
                                           min_tracking_confidence=MIN_TRACKING_CONFIDENCE)



    logging.info("Initialising video capture...")
    video_capture = cv2.VideoCapture()
    success = video_capture.open(0, 0)
    if not success:
        raise RuntimeError("Error opening video capture - perhaps the video capture target or backend is invalid.")
    # for prop_id, prop_value in VIDEO_CAPTURE_PROPERTIES.items():
    #     supported = video_capture.set(prop_id, prop_value)
    #     if not supported:
    #         logging.warning(f"Property id {prop_id} is not supported by video capture backend {VIDEO_CAPTURE_BACKEND}.")

    logging.info("Initialisation done.")

    # basic opencv + mediapipe stuff from https://www.youtube.com/watch?v=v-ebX04SNYM
    cv2.namedWindow("Hotspots", cv2.WINDOW_FULLSCREEN)

    #TODO: use capture class to take photos on another thread
    while video_capture.isOpened():
        success, video_capture_img = video_capture.read()
        if not success:
            logging.warning("Unsuccessful video read; ignoring frame.")
            continue

        camera_height, camera_width, _ = video_capture_img.shape

        # run model
        video_capture_img_rgb = cv2.cvtColor(video_capture_img, cv2.COLOR_BGR2RGB)  # convert to RGB
        model_output = hands_model.process(video_capture_img_rgb)

        image=np.full(screenDimensions, 0,np.uint8) #generate empty background
        if hasattr(model_output, "multi_hand_landmarks") and model_output.multi_hand_landmarks is not None:
            # update hotspots
            fingertip_coords = [landmarks.landmark[i] for i in FINGERTIP_INDICES for landmarks in
                                model_output.multi_hand_landmarks]

            for hotspot in hotspots:
                hotspot_just_activated = hotspot.update(fingertip_coords)

                if hotspot_just_activated:
                    # make sure there's no other active hotspots
                    for other_hotspot in hotspots:
                        if other_hotspot == hotspot:
                            continue
                        other_hotspot.deactivate()


            # #draw fingertips
            index_coords = [calibrator.norm_to_proj((landmarks.landmark[8].x, landmarks.landmark[8].y)) for landmarks in
                                model_output.multi_hand_landmarks]
            #transform coords with matrix and draw circle
            for index_coord in index_coords:
                print([landmarks.landmark[8]  for landmarks in
                                model_output.multi_hand_landmarks])
                # index_coord = calibrator.inverseTransform(index_coord)
                # index_coord = (int(index_coord[0]), int(index_coord[1]))
                cv2.circle(image, index_coord, 3, 255, 2)


        # draw hotspot
        for hotspot in hotspots:
            hotspot.draw(image)

        cv2.imshow("Hotspots", image)

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


def demo():
    class MyEventListener(EventListener):
        def OnPressDetected(self, hotspot_id):
            print(f"Hotspot {hotspot_id} activated.")
    eventLister = MyEventListener()

    SKIP_CALIBRATION = False

    calibrator = Calibrator((1080, 1920))
    if SKIP_CALIBRATION:
        calibrator.skipCalibration()
    else:
        calibrator.calibrate()
        if calibrator._tMatrix is None:
            exit()

    hotspots = [Hotspot(0, (700, 700), calibrator, eventLister), Hotspot(1, (100, 100), calibrator, eventLister)]

    run2((1080, 1920), hotspots, calibrator)


if __name__ == "__main__":
    demo()
