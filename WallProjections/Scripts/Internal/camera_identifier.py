import multiprocessing
import cv2

MAX_INDEX = 20
TIMEOUT = 5


def open_camera(i, return_queue):
    try:
        cap = cv2.VideoCapture(i)
        if cap.isOpened():
            return_queue.put(i)
            return
    except cv2.Error as e:
        return


def identify_cameras():
    return_queue = multiprocessing.Queue()
    for i in range(MAX_INDEX):
        p = multiprocessing.Process(target=open_camera, args=(i, return_queue))
        p.start()
        p.join(timeout=TIMEOUT)
        if p.is_alive():
            p.terminate()
            p.join()

    if not return_queue.empty():
        print(return_queue.get())
    else:
        print("empty")


if __name__ == '__main__':
    identify_cameras()
