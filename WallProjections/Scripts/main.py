import cv2
import mediapipe as mp
import logging
from abc import ABC, abstractmethod
from typing import NamedTuple
import math
import time
from tkinter import *
from tkinter import ttk
from threading import Thread
from PIL import Image, ImageTk


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

VIDEO_CAPTURE_TARGET: int | str = 0
"""Camera ID, video filename, image sequence filename or video stream URL to capture video from.

Set to 0 to use default camera.

See https://docs.opencv.org/3.4/d8/dfe/classcv_1_1VideoCapture.html#a949d90b766ba42a6a93fe23a67785951 for details."""

VIDEO_CAPTURE_BACKEND: int = cv2.CAP_ANY
"""The API backend to use for capturing video. 

Use ``cv2.CAP_ANY`` to auto-detect. 

See https://docs.opencv.org/3.4/d4/d15/group__videoio__flags__base.html for more options."""

VIDEO_CAPTURE_PROPERTIES: dict[int, int] = {}
"""Extra properties to use for OpenCV video capture.

Tweaking these properties could be found useful in case of any issues with capturing video.

See https://docs.opencv.org/3.4/d4/d15/group__videoio__flags__base.html#gaeb8dd9c89c10a5c63c139bf7c4f5704d and
https://docs.opencv.org/3.4/dc/dfc/group__videoio__flags__others.html."""

BUTTON_COOLDOWN: float = 1.5
"""The number of seconds for which the finger must be over the hotspot until the 
button activates. Once the finger leaves the hotspot, it also takes this amount
of time to \"cool down\"."""


class EventListener(ABC):
    @abstractmethod
    def OnPressDetected(self, hotspot_id: int) -> None:
        pass


class Point(NamedTuple):
    x: float
    y: float


class _Button:
    """
    The _Button class is used to keep track if a HotSpot is activated
    """

    def __init__(self):
        self.finger_is_over: bool = False
        """Whether or not a finger is touching the HotSpot."""

        self.prev_time: float = time.time()
        """The last time when `finger_is_over` switched value."""

        self.prev_pressed_amount: float = 0
        """The value of `get_pressed_amount` at the last time when `finger_is_over` switched value."""

    def press(self) -> bool:
        """
        Called every frame the HotSpot is pressed,
        returns a Bool indicating if the button has been pressed for long enough.
        """
        if not self.finger_is_over:
            self.prev_pressed_amount = self.get_pressed_amount()
            self.prev_time = time.time()
            self.finger_is_over = True

        return self.get_pressed_amount() == 1

    def un_press(self) -> None:
        """
        Called every frame the HotSpot is not pressed.
        """
        if self.finger_is_over:
            self.prev_pressed_amount = self.get_pressed_amount()
            self.prev_time = time.time()
            self.finger_is_over = False

    def get_pressed_amount(self) -> float:
        """
        Returns a value between 0 (fully unpressed) and 1 (fully pressed.)
        """
        time_since_last_change = time.time() - self.prev_time
        if self.finger_is_over:
            return min(1.0, self.prev_pressed_amount + time_since_last_change / BUTTON_COOLDOWN)
        else:
            return max(0.0, self.prev_pressed_amount - time_since_last_change / BUTTON_COOLDOWN)


class Hotspot:
    """
    Represents a hotspot. A hotspot is a circular area on the screen that can be pressed.
    """

    def __init__(self, hotspot_id: int, x: float, y: float, event_listener: EventListener, radius: float = 0.03):
        self.id: int = hotspot_id
        self._x: float = x
        self._y: float = y
        self._radius: float = radius
        self._event_listener: EventListener = event_listener
        self._button: _Button = _Button()
        self._is_activated: bool = False
        self._time_activated: float = 0

    def draw(self, img) -> None:
        img_width = img.shape[1]
        img_height = img.shape[0]

        # Draw outer ring
        cv2.circle(img, self._onscreen_xy(img_width, img_height), self._onscreen_radius(), (255, 255, 255),
                   thickness=2)

        # Draw inner circle
        if self._button.get_pressed_amount() != 0:
            cv2.circle(img, self._onscreen_xy(img_width, img_height), self._inner_radius(), (255, 255, 255),
                       thickness=-1)

    def _onscreen_xy(self, screen_width: int, screen_height: int) -> tuple[int, int]:
        """
        Takes in screen resolution and outputs pixel coordinates of hotspot
        """

        return int(screen_width * self._x), int(screen_height * self._y)

    def _onscreen_radius(self) -> int:
        """
        Returns on screen pixel radius from "mediapipes" radius
        """
        # Const value 800 gives a visual HotSpot with a slightly larger "hitbox" than drawn ring
        return int(800 * self._radius)

    def _inner_radius(self) -> int:
        radius = self._onscreen_radius() * self._button.get_pressed_amount()
        if self._is_activated:
            radius += math.sin((time.time() - self._time_activated) * 3) * 6 + 6
        return int(radius)

    def update(self, points: list[Point]) -> bool:
        if self._is_activated:
            pass

        else:  # not activated
            # check if any fingertips inside of hotspot
            finger_inside = False
            for point in points:
                if self._is_point_inside(point):
                    finger_inside = True

            if finger_inside:
                if self._button.press():
                    # button has been pressed for long enough
                    self.activate()
                    return True
            else:
                self._button.un_press()

        return False

    def _is_point_inside(self, point: Point) -> bool:
        """
        Returns true if given point is inside hotspot
        """
        squared_dist = (self._x - point.x) ** 2 + (self._y - point.y) ** 2
        return squared_dist <= self._radius ** 2

    def activate(self) -> None:
        self._time_activated = time.time()
        self._is_activated = True

    def deactivate(self) -> None:
        self._is_activated = False


hotspots: list[Hotspot] = []
"""The global list of hotspots."""


def run(event_listener: EventListener) -> None:  # This function is called by Program.cs
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

            for hotspot in hotspots:
                hotspot_just_activated = hotspot.update(fingertip_coords)

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
        for hotspot in hotspots:
            hotspot.draw(video_capture_img)

        cv2.imshow("Projected Hotspots", video_capture_img)

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


# -------- SET UP/CALIBRATE HOTSPOTS --------

class VideoCaptureThread(Thread):
    def __init__(self):
        super().__init__()
        self.current_frame = None
        self.stopping = False

    def run(self):
        logging.info("Initialising video capture...")
        video_capture = cv2.VideoCapture()
        success = video_capture.open(VIDEO_CAPTURE_TARGET, VIDEO_CAPTURE_BACKEND)
        if not success:
            raise RuntimeError("Error opening video capture - perhaps the video capture target or backend is invalid.")
        for prop_id, prop_value in VIDEO_CAPTURE_PROPERTIES.items():
            supported = video_capture.set(prop_id, prop_value)
            if not supported:
                logging.warning(f"Property id {prop_id} is not supported by video capture backend {VIDEO_CAPTURE_BACKEND}.")

        while video_capture.isOpened():
            success, video_capture_img = video_capture.read()
            if not success:
                logging.warning("Unsuccessful video read; ignoring frame.")
                continue

            video_capture_img = cv2.cvtColor(video_capture_img, cv2.COLOR_BGR2RGB)  # convert to RGB
            new_dim = (int(video_capture_img.shape[1] / video_capture_img.shape[0] * 480), 480)
            video_capture_img = cv2.resize(video_capture_img, new_dim, interpolation=cv2.INTER_NEAREST)  # normalise size
            self.current_frame = ImageTk.PhotoImage(Image.fromarray(video_capture_img))

            if self.stopping:
                break

        video_capture.release()

    def stop(self):
        """
        Stop the video capture after the current frame.

        Call `Thread.join` after this to block until the thread has stopped.
        """
        self.stopping = True


class SetUpHotspotsWindow(Tk):
    def __init__(self):
        super().__init__()
        self.title("Set up hotspots")
        frm = ttk.Frame(self, padding=16)
        frm.grid()
        self._infoLabel = ttk.Label(frm, text="Use your mouse to place the hotspot in the desired position.")
        self._infoLabel.grid(column=0, row=0)
        self._videoLabel = ttk.Label(frm)
        self._videoLabel.grid(column=0, row=1)
        ttk.Button(frm, text="Cancel", command=self.cancel).grid(column=0, row=2)
        ttk.Button(frm, text="Ok", command=self.ok).grid(column=1, row=2)

        self._displayed_frame = None  # store reference to frame so it doesn't get GCed
        self._videoCaptureThread = VideoCaptureThread()
        self._videoCaptureThread.start()
        self._poll_video_capture()

    def _poll_video_capture(self):
        if self._videoCaptureThread.current_frame is not None and \
                self._videoCaptureThread.current_frame != self._displayed_frame:
            # noinspection PyUnusedLocal
            old_displayed_frame = self._displayed_frame  # keep reference until videoLabel updated
            self._displayed_frame = self._videoCaptureThread.current_frame
            self._videoLabel.config(image=self._displayed_frame)
        self.after(33, self._poll_video_capture)  # 1000 / 33 = 30 fps

    def cancel(self):
        self._videoCaptureThread.stop()
        self._videoCaptureThread.join(timeout=1)
        self.destroy()

    def ok(self):
        self._videoCaptureThread.stop()
        self._videoCaptureThread.join()
        # todo: save hotspots
        self.destroy()


def set_up_hotspots():
    set_up_hotspots_window = SetUpHotspotsWindow()
    set_up_hotspots_window.mainloop()


if __name__ == "__main__":
    # class MyEventListener(EventListener):
    #     def OnPressDetected(self, hotspot_id):
    #         print(f"Hotspot {hotspot_id} activated.")
    #
    # run(MyEventListener())

    set_up_hotspots()
