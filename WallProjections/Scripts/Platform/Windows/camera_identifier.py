import json
import cv2
from pygrabber.dshow_graph import FilterGraph
# noinspection PyPackages
from .Helper.logger import setup_logger

logger = setup_logger("camera_identifier")


def get_cameras() -> str:
    indices: dict[int, str] = {}
    devices = FilterGraph().get_input_devices()
    for device_index, device_name in enumerate(devices):
        try:
            cam = cv2.VideoCapture(device_index, cv2.CAP_DSHOW)
            if cam.isOpened():
                indices[device_index] = device_name
                cam.release()
        except Exception:
            continue

    logger.info(f"Identified {len(indices)} cameras.")
    return json.dumps(indices)
