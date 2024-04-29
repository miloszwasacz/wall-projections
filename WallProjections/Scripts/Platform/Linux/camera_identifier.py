import glob
import json
import cv2
from Scripts.Helper.logger import setup_logger

logger = setup_logger("camera_identifier")


def get_cameras() -> str:
    camera_indices: dict[int, str] = {}
    cv2.setLogLevel(0)
    devices = glob.glob('/dev/video*')
    for device_name, device in enumerate(devices):
        try:
            # Extract the index from the device file path
            index = int(device.lstrip('/dev/video'))
            cam = cv2.VideoCapture(index)
            if cam.isOpened():
                camera_indices[index] = f"Camera {device_name + 1}"
                cam.release()
        except Exception:
            continue

    logger.info(f"Identified {len(camera_indices)} cameras.")
    return json.dumps(camera_indices)
