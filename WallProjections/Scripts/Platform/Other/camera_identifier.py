import concurrent.futures
import json
import cv2
from Scripts.Helper.logger import setup_logger

MAX_INDEX = 1000
TIMEOUT = 5

logger = setup_logger("camera_identifier")


def open_camera(i) -> int | None:
    try:
        cap = cv2.VideoCapture()
        cap.setExceptionMode(True)
        cap.open(i)
        if cap.isOpened():
            cap.release()
            return i
    except cv2.error:
        return None
    except Exception:
        return None


def get_cameras() -> str:
    camera_indices: dict[int, str] = {}
    cv2.setLogLevel(0)
    with concurrent.futures.ThreadPoolExecutor() as executor:
        futures = [executor.submit(open_camera, i) for i in range(MAX_INDEX)]
        for future in concurrent.futures.as_completed(futures, timeout=TIMEOUT):
            index = future.result()
            if index is not None:
                camera_indices[index] = f"Camera {len(camera_indices) + 1}"

    logger.info(f"Identified {len(camera_indices)} cameras.")
    return json.dumps(camera_indices)
