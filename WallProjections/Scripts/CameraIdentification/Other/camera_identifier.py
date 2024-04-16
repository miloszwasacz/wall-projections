import concurrent.futures
import json
import os
import sys
import cv2
# noinspection PyPackages
from .Helper.logger import setup_logger

MAX_INDEX = 1000
TIMEOUT = 5

logger = setup_logger("camera_identifier")


# Suppresses the console output
class ConsoleSuppressor:
    def __enter__(self):
        self.original_stdout_fd = sys.stdout.fileno()
        self.original_stderr_fd = sys.stderr.fileno()

        # Save a copy of the original file descriptors
        self.saved_stdout_fd = os.dup(self.original_stdout_fd)
        self.saved_stderr_fd = os.dup(self.original_stderr_fd)

        with open(os.devnull, 'w') as devnull:
            # Redirect stdout and stderr to /dev/null
            os.dup2(devnull.fileno(), self.original_stdout_fd)
            os.dup2(devnull.fileno(), self.original_stderr_fd)

    def __exit__(self, exc_type, exc_val, exc_tb):
        # Restore the original file descriptors
        os.dup2(self.saved_stdout_fd, self.original_stdout_fd)
        os.dup2(self.saved_stderr_fd, self.original_stderr_fd)


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
    with ConsoleSuppressor():  # There is a lot of unnecessary console output from OpenCV
        with concurrent.futures.ThreadPoolExecutor() as executor:
            futures = [executor.submit(open_camera, i) for i in range(MAX_INDEX)]
            for future in concurrent.futures.as_completed(futures, timeout=TIMEOUT):
                index = future.result()
                if index is not None:
                    camera_indices[index] = f"Camera {len(camera_indices) + 1}"

    logger.info(f"Identified {len(camera_indices)} cameras.")
    return json.dumps(camera_indices)
