import cv2
import mediapipe as mp

hands_model = mp.solutions.hands.Hands()
webcam = cv2.VideoCapture(0)


HOTSPOT_SCREEN_COORDS = (320, 240)
FINGERTIP_INDICES = (4, 8, 12, 16, 20)


class HotSpot():
    def __init__(self, id, x, y, radius):
        self.id = id
        self.x = x
        self.y = y
        self.radius = radius
    
    def isPointInside(self, point):
        """
        Returns true if given point is inside hotspot
        """
        squaredDist = (self.x - point.x)**2 + (self.y - point.y)**2
        return squaredDist <= self.radius**2

hotSpots = [HotSpot(1, 0.5, 0.5, 0.01)]

def detect_buttons(event_handler): #This function is called by Program.cs
    while webcam.isOpened():
        success, img = webcam.read()

        # run model
        img_rbg = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        model_output = hands_model.process(img_rbg)

        # detect if finger over hotspot
        if model_output.multi_hand_landmarks:
            fingertip_coords = [landmarks.landmark[i] for i in FINGERTIP_INDICES for landmarks in model_output.multi_hand_landmarks]
            for fingertip_coord in fingertip_coords:
                for hotSpot in hotSpots:
                    if hotSpot.isPointInside(fingertip_coord):
                        event_handler.OnPressDetected(hotSpot.id)


        # annotate hand landmarks and hotspot onscreen
        cv2.circle(img, HOTSPOT_SCREEN_COORDS, 10, (255, 0, 255), thickness=2)
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