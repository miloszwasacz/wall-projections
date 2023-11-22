import cv2
import mediapipe as mp
from HotSpot import HotSpot

hands_model = mp.solutions.hands.Hands()
webcam = cv2.VideoCapture(0)


FINGERTIP_INDICES = (4, 8, 12, 16, 20)


def detect_buttons(event_handler): #This function is called by Program.cs
    #Load hotspots
    hotSpots = [HotSpot(0, 0.5, 0.5, event_handler), HotSpot(1, 0.8, 0.8, event_handler)]
    
    while webcam.isOpened():
        success, img = webcam.read()
        height, width, _ = img.shape
    

        # run model
        img_rbg = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        model_output = hands_model.process(img_rbg)

        # detect if finger over hotspot
        if model_output.multi_hand_landmarks:
            fingertip_coords = [landmarks.landmark[i] for i in FINGERTIP_INDICES for landmarks in model_output.multi_hand_landmarks]
            for hotSpot in hotSpots:
                hotSpot.update(fingertip_coords)


        # annotate hand landmarks and hotspot onscreen
        for hotSpot in hotSpots:
            hotSpot.draw(img, cv2, width, height)
        if model_output.multi_hand_landmarks is not None:
            for landmarks in model_output.multi_hand_landmarks:
                mp.solutions.drawing_utils.draw_landmarks(img, landmarks, connections=mp.solutions.hands.HAND_CONNECTIONS)

        cv2.imshow("Hand Tracking Testing", img)


        if cv2.waitKey(5) & 0xFF == ord("q"):
            break

    webcam.release()
    cv2.destroyAllWindows()

if __name__ == "__main__":
    class EventHandler:
        def OnPressDetected(self, id):
            print("button pressed", id)
    detect_buttons(EventHandler())