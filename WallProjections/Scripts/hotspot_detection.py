import threading

import cv2
import mediapipe as mp
from Scripts.Helper.EventHandler import EventHandler
from Scripts.Helper.Hotspot import Hotspot
from Scripts.Helper.logger import setup_logger
from Scripts.Interop import numpy_dotnet_converters as npnet
from Scripts.Interop.json_dict_converters import json_to_3dict
from Scripts.calibration import Calibrator
from Scripts.video_capture_factory import getVideoCapture

logger = setup_logger("hotspot_detection")

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

detection_running = False
detection_mutex = threading.Lock()


def generate_hotspots(
        event_handler: EventHandler,
        hotspot_coords_str: str
) -> list[Hotspot]:
    hotspots_coords = json_to_3dict(hotspot_coords_str)

    hotspots: list[Hotspot] = []
    for hotspot_id, hotspot_coord_rad in hotspots_coords.items():
        coords = hotspot_coord_rad[0], hotspot_coord_rad[1]
        radius = hotspot_coord_rad[2]
        hotspot = Hotspot(hotspot_id, coords, event_handler, radius=radius)
        hotspots.append(hotspot)
    return hotspots


def hotspot_detection(
        video_capture_target: int | str,
        event_handler: EventHandler,
        calibration_matrix_net_array,
        hotspot_coords_str: str
) -> None:
    """
    Given hotspot projector coords, a transformation matrix and an event_handler
    calls events when hotspots are pressed or unpressed
    """
    global detection_running

    acquired = detection_mutex.acquire(timeout=1)
    if not acquired:
        logger.error("Failed to acquire mutex for hotspot detection (it's probably already running).")
        return

    detection_running = True

    logger.info("Starting hotspot detection.")

    # initialise ML hand-tracking model
    logger.info("Initialising hand-tracking model.")
    hands_model = mp.solutions.hands.Hands(max_num_hands=MAX_NUM_HANDS,
                                           min_detection_confidence=MIN_DETECTION_CONFIDENCE,
                                           min_tracking_confidence=MIN_TRACKING_CONFIDENCE)

    # initialise video capture
    video_capture = getVideoCapture(video_capture_target)
    video_capture.start()

    # initialise calibrator
    h, w, d = video_capture.get_current_frame().shape
    logger.info(f"Camera width: {w}, height: {h}")
    calibration_matrix = npnet.asNumpyArray(calibration_matrix_net_array)
    calibrator = Calibrator(calibration_matrix, (w, h))
    hotspots = generate_hotspots(event_handler, hotspot_coords_str)

    logger.info("Hotspot detection started.")

    while detection_running:
        video_capture_img_bgr = video_capture.get_current_frame()
        video_capture_img_rgb = cv2.cvtColor(video_capture_img_bgr, cv2.COLOR_BGR2RGB)

        # run model
        model_output = hands_model.process(video_capture_img_rgb)

        # noinspection PyUnresolvedReferences
        if hasattr(model_output, "multi_hand_landmarks") and model_output.multi_hand_landmarks is not None:
            # update hotspots
            fingertip_coords_norm = [landmarks.landmark[i] for i in FINGERTIP_INDICES for landmarks in
                                     model_output.multi_hand_landmarks]
            fingertip_coords_proj = [calibrator.norm_to_proj((fingertip.x, fingertip.y)) for fingertip in
                                     fingertip_coords_norm]
            for hotspot in hotspots:
                hotspot.update(fingertip_coords_proj)

    # clean up
    video_capture.stop()
    cv2.destroyAllWindows()  # there shouldn't be any windows but just in case
    hands_model.close()
    detection_mutex.release()


def stop_hotspot_detection():
    global detection_running
    logger.info("Stopping hotspot detection.")
    detection_running = False
    detection_mutex.acquire()  # wait until hotspot detection has stopped
    detection_mutex.release()
    logger.info("Hotspot detection stopped.")
