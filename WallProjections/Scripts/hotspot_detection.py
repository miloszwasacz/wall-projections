import logging
import cv2
import mediapipe as mp

# noinspection PyPackages
from .Helper import numpy_dotnet_converters as npnet
# noinspection PyPackages
from .Helper.EventHandler import EventHandler
# noinspection PyPackages
from .Helper.Hotspot import Hotspot
# noinspection PyPackages
from .Helper.VideoCaptureThread import VideoCaptureThread
# noinspection PyPackages
from .calibration import Calibrator

logging.basicConfig(level=logging.INFO)
detection_running = False

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


def generate_hotspots(
        event_handler: EventHandler,
        calibration_matrix_net_array,
        hotspot_coords_str: str
) -> list[Hotspot]:
    photo = VideoCaptureThread.take_photo()
    w, h, d = photo.shape

    calibration_matrix = npnet.asNumpyArray(calibration_matrix_net_array)
    calibrator = Calibrator(calibration_matrix, (w, h))
    hotspots_coords = npnet.json_to_dict(hotspot_coords_str)

    hotspots: list[Hotspot] = []
    for hotspot_id, hotspot_coord_rad in hotspots_coords.items():
        coords = hotspot_coord_rad[0], hotspot_coord_rad[1]
        radius = hotspot_coord_rad[2]
        hotspot = Hotspot(hotspot_id, coords, calibrator, event_handler, radius=radius)
        hotspots.append(hotspot)
    return hotspots


def hotspot_detection(event_handler: EventHandler, calibration_matrix_net_array, hotspot_coords_str: str) -> None:
    """
    Given hotspot projector coords, a transformation matrix and an event_handler
    calls events when hotspots are pressed or unpressed
    """
    global detection_running
    detection_running = True

    hotspots = generate_hotspots(event_handler, calibration_matrix_net_array, hotspot_coords_str)

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

    # TODO: use capture class to take photos on another thread
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

            for hotspot in hotspots:
                hotspot.update(fingertip_coords)


def stop_hotspot_detection():
    logging.info("stopping hotspot detection")
    global detection_running
    detection_running = False
    pass
