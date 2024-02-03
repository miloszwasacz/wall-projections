from calibrate import *
from main import *
import numpy as np
import cv2
from cv2 import aruco
from typing import Dict, Tuple


hotspots: list[Hotspot] = []
"""The global list of hotspots."""

def transformFingertips(fingertips, matrix):
    newFingertips = []
    for fingertip in fingertips:
        newFingertips.append(cv2.warpPerspective(np.array(fingertip), matrix, (1080,1920)))
    
    return newFingertips

def run2(event_listener: EventListener, transformMatrix) -> None:  # This function is called by Program.cs
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

    hotspots = [Hotspot(0, 0.5, 0.5, event_listener), Hotspot(1, 0.8, 0.8, event_listener)]

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

        if hasattr(model_output, "multi_hand_landmarks") and model_output.multi_hand_landmarks is not None:
            # update hotspots
            fingertip_coords = [landmarks.landmark[i] for i in FINGERTIP_INDICES for landmarks in
                                model_output.multi_hand_landmarks]

            transformedFingertips = transformFingertips(fingertip_coords, transformMatrix)
            for hotspot in hotspots:
                hotspot_just_activated = hotspot.update(transformedFingertips)

                if hotspot_just_activated:
                    # make sure there's no other active hotspots
                    for other_hotspot in hotspots:
                        if other_hotspot == hotspot:
                            continue
                        other_hotspot.deactivate()

                    event_listener.OnPressDetected(hotspot.id)

            # draw hand landmarks
            for landmarks in model_output.multi_hand_landmarks:
                mp.solutions.drawing_utils.draw_landmarks(video_capture_img, landmarks,
                                                          connections=mp.solutions.hands.HAND_CONNECTIONS)

        # draw hotspot
        image=np.full((1080,1920), 0,np.uint8) #generate empty background
        # draw hotspot
        for hotspot in hotspots:
            hotspot.draw(image)

        cv2.imshow("Projected Hotspots", image)

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


def media_finished() -> None:
    for hotspot in hotspots:
        hotspot.deactivate()



if __name__ == "__main__":
    #generate an example projectCoords dictionary
    projectedCoords = {}
    k = 0
    for i in range(6):
        for j  in range(12):
            projectedCoords[k] = (25+(150*i), 25+(150*j))
            k = k+1 
    
    arucoDict = aruco.getPredefinedDictionary(aruco.DICT_7X7_100)

    cv2.namedWindow("Calibration", cv2.WINDOW_FULLSCREEN)
    cv2.imshow("Calibration", drawArucos(projectedCoords, arucoDict))
    cv2.waitKey(800)

    cameraCoords = detectArucos(takePhoto(), arucoDict, displayResults=False)

    camera2ProjectorMatrix = getTransformationMatrix(cameraCoords, projectedCoords)

    class MyEventListener(EventListener):
        def OnPressDetected(self, hotspot_id):
            print(f"Hotspot {hotspot_id} activated.")

    run2(MyEventListener(), camera2ProjectorMatrix)
