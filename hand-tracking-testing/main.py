import cv2
import mediapipe as mp
import winsound

hands_model = mp.solutions.hands.Hands()
webcam = cv2.VideoCapture(0)

HOTSPOT_COORDS = (0.5, 0.5)
HOTSPOT_SCREEN_COORDS = (320, 240)
FINGERTIP_INDICES = (4, 8, 12, 16, 20)

hotspot_was_pressed = False

while webcam.isOpened():
    success, img = webcam.read()

    # run model
    img_rbg = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    model_output = hands_model.process(img_rbg)

    # detect if finger over hotspot
    if model_output.multi_hand_landmarks is not None:
        hotspot_pressed = False

        for landmarks in model_output.multi_hand_landmarks:
            for i in FINGERTIP_INDICES:
                fingertip_coords = landmarks.landmark[i]
                if (fingertip_coords.x - HOTSPOT_COORDS[0])**2 + (fingertip_coords.y - HOTSPOT_COORDS[1])**2 <= 0.001:
                    hotspot_pressed = True
                    if not hotspot_was_pressed:
                        winsound.Beep(1000, 500)

        hotspot_was_pressed = hotspot_pressed

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
