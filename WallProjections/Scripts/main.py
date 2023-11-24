import cv2
import mediapipe as mp

from EventListener import EventListener
from Hotspot import Hotspot


FINGERTIP_INDICES = (4, 8, 12, 16, 20)
"""The indices for the thumb fingertip, index fingertip, ring fingertip, etc."""

hotspots: list[Hotspot] = []
"""The global list of hotspots."""


def run(event_listener) -> None:  # This function is called by Program.cs
    """
    Captures video and runs the hand-detection model to handle the hotspots.
    """

    global hotspots

    hands_model = mp.solutions.hands.Hands()
    video_capture = cv2.VideoCapture(0)

    hotspots = [Hotspot(0, 0.5, 0.5, event_listener), Hotspot(1, 0.8, 0.8, event_listener)]

    # basic opencv + mediapipe stuff from https://www.youtube.com/watch?v=v-ebX04SNYM

    while video_capture.isOpened():
        success, video_capture_img = video_capture.read()
        if not success:
            print("Ignoring empty camera frame.")
            continue

        camera_height, camera_width, _ = video_capture_img.shape

        # run model
        video_capture_img_rgb = cv2.cvtColor(video_capture_img, cv2.COLOR_BGR2RGB)
        model_output = hands_model.process(video_capture_img_rgb)

        if not hasattr(model_output, "multi_hand_landmarks"):
            raise RuntimeError("Model output does not have multi_hand_landmarks attribute.")

        # detect if finger over hotspot
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

                event_listener.on_hotspot_activated(hotspot.id)

        # annotate hand landmarks and hotspot onscreen
        for hotspot in hotspots:
            hotspot.draw(video_capture_img, cv2, camera_width, camera_height)
        for landmarks in model_output.multi_hand_landmarks:
            mp.solutions.drawing_utils.draw_landmarks(video_capture_img, landmarks,
                                                      connections=mp.solutions.hands.HAND_CONNECTIONS)

        cv2.imshow("Projected Hotspots", video_capture_img)

        key = cv2.waitKey(1) & 0xFF
        if key == ord("c"):
            media_finished()
        elif key == ord("q"):
            break

    # clean up
    video_capture.release()
    cv2.destroyAllWindows()
    hands_model.close()


def media_finished():
    for hotspot in hotspots:
        hotspot.deactivate()


if __name__ == "__main__":
    class MyEventListener(EventListener):
        def on_hotspot_activated(self, hotspot_id):
            print(f"Hotspot {hotspot_id} activated.")

    run(MyEventListener())
