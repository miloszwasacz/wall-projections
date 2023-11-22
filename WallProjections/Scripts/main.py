import cv2
import mediapipe as mp

from EventListener import EventListener
from HotSpot import HotSpot


FINGERTIP_INDICES = (4, 8, 12, 16, 20)
"""The indices for the thumb fingertip, index fingertip, ring fingertip, etc."""

hotspots = []
"""The global list of hotspots."""


def run(event_listener):  # This function is called by Program.cs
    """
    Captures video and runs the hand-detection model to handle the hotspots.
    """

    global hotspots

    hands_model = mp.solutions.hands.Hands()
    video_capture = cv2.VideoCapture(0)

    # Load hotspots
    hotspots = [HotSpot(0, 0.5, 0.5, event_listener), HotSpot(1, 0.8, 0.8, event_listener)]

    # basic opencv + mediapipe stuff from https://www.youtube.com/watch?v=v-ebX04SNYM

    while video_capture.isOpened():
        success, img = video_capture.read()
        height, width, _ = img.shape

        # run model
        img_rbg = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        model_output = hands_model.process(img_rbg)

        # detect if finger over hotspot
        if model_output.multi_hand_landmarks:
            fingertip_coords = [landmarks.landmark[i] for i in FINGERTIP_INDICES for landmarks in
                                model_output.multi_hand_landmarks]

            for hotSpot in hotspots:
                hotspotJustActivated = hotSpot.update(fingertip_coords)

                if hotspotJustActivated:
                    # make sure there's no other active hotspots
                    for otherHotspot in hotspots:
                        if otherHotspot == hotSpot:
                            continue
                        otherHotspot.deactivate()

                    event_listener.on_hotspot_activated(hotSpot.id)

        # annotate hand landmarks and hotspot onscreen
        for hotSpot in hotspots:
            hotSpot.draw(img, cv2, width, height)
        if model_output.multi_hand_landmarks is not None:
            for landmarks in model_output.multi_hand_landmarks:
                mp.solutions.drawing_utils.draw_landmarks(img, landmarks,
                                                          connections=mp.solutions.hands.HAND_CONNECTIONS)

        cv2.imshow("Hand Tracking Testing", img)

        if cv2.waitKey(5) & 0xFF == ord("c"):
            media_finished()
        if cv2.waitKey(5) & 0xFF == ord("q"):
            break

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
