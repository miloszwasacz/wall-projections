import abc
import logging
import numpy as np
import cv2
from cv2 import aruco
import mediapipe as mp
from main import *
from typing import Dict, List, NamedTuple, Tuple

#EVENT LISTENER

class EventListener(abc.ABC):
    @abc.abstractmethod
    def OnPressDetected(self, hotspot_id: int) -> None:
        pass

#CALIBRATION  

def takePhoto() -> np.ndarray:
    """
    Returns a photo from a detectable camera
    """
    projectedCoords = {}
    code = 0
    for i in range(6):
        for j  in range(12):
            projectedCoords[code] = (25+(150*i), 25+(150*j))
            code = code+1 
    
    arucoDict = aruco.getPredefinedDictionary(aruco.DICT_7X7_100)

    videoCaptureThread = VideoCaptureThread()
    videoCaptureThread.start()
    image = None
    while image is None:
        image = videoCaptureThread.current_frame
        cv2.waitKey(500)
    videoCaptureThread.stop()
    videoCaptureThread.join()
    
    return image

class Calibrator:

    def __init__(self, screenDimensions : tuple[int, int]):
        self._screenWidth = screenDimensions[0]
        self._screenHeight =  screenDimensions[1]
        self._cameraWidth = self._cameraHeight = None
        self._tMatrix = self._inverseTMatrix = None 
        
    
    def skipCalibration(self):
        self._tMatrix = self._inverseTMatrix = np.identity(3)
        
        
    def calibrate(self, displayResults : bool = False):    
        projectedCoords = {}
        code = 0
        for i in range(6):
            for j  in range(12):
                projectedCoords[code] = (25+(150*i), 25+(150*j))
                code = code+1 
        
        arucoDict = aruco.getPredefinedDictionary(aruco.DICT_7X7_100)

        cv2.namedWindow("Hotspots", cv2.WINDOW_FULLSCREEN)

        cv2.imshow("Hotspots", self._drawArucos(projectedCoords, arucoDict))
        cv2.waitKey(800)
        photo = takePhoto()
        self._cameraWidth, self._cameraHeight, _ = photo.shape
        cameraCoords = self._detectArucos(photo, arucoDict, displayResults=displayResults)
        
        self._tMatrix = self._getTransformationMatrix(projectedCoords, cameraCoords)
        self._inverseTMatrix = self._getTransformationMatrix(cameraCoords, projectedCoords)

    def _drawArucos(self, projectedCoords : dict[int, tuple[int, int]], arucoDict : aruco.Dictionary) -> np.ndarray:
        """
        Returns a white image with an ArUco drawn (with the corresponding code,
        at the corresponding coord) for each entry in projectedCoords
        """
        image=np.full((1080,1920), 255,np.uint8) #generate empty background
        for code in projectedCoords: #id is the name of a built in function in python, so use iD
            arucoImage = aruco.generateImageMarker(arucoDict, code, 100, borderBits= 1) #fetch ArUco
            topLeftCorner = projectedCoords[code]
            #replace pixels in image with the ArUco's pixels
            image[topLeftCorner[0]:topLeftCorner[0]+arucoImage.shape[0], topLeftCorner[1]:topLeftCorner[1]+arucoImage.shape[1]] = arucoImage
        return image

    def _detectArucos(self, img : np.ndarray, arucoDict : aruco.Dictionary, displayResults = False) -> dict[int, tuple[np.float32, np.float32]]:
        """
        Detects ArUcos in an image and returns a dictionary of codes to coordinates

        :param displayResults: displays the an img with detected ArUcos annotated on top

        """
        corners, ids, _ = aruco.detectMarkers(img, arucoDict, parameters=aruco.DetectorParameters())
        
        if displayResults == True:
            cv2.imshow("Labled Aruco Markers", aruco.drawDetectedMarkers(img, corners, ids))
            cv2.waitKey(60000)
        
        detectedCoords = {}
        if ids is None:
            return detectedCoords
        for i in range(ids.size):
            iD = ids[i][0]
            topLeftCorner = (corners[i][0][0][0], corners[i][0][0][1])
            detectedCoords[iD] = topLeftCorner
        return detectedCoords


    def _getTransformationMatrix(self, fromCoords : dict[int, tuple[int, int]], toCoords : dict[int, tuple[np.float32, np.float32]]) -> np.ndarray:
        """
        Returns a transformation matrix from the coords stored in one dictionary to another
        """
        fromArray = []
        toArray = []
        for iD in fromCoords:
            if iD in toCoords: #if iD in both dictionaries
                fromArray.append(fromCoords[iD])
                toArray.append(toCoords[iD])

        if len(fromArray) < 4:
            print(len(fromArray), "matching coords found, at least 4 are required for calibration.")
            return
        
        fromNpArray = np.array(fromArray, dtype=np.float32)
        toNpArray = np.array(toArray, dtype=np.float32)

        return cv2.findHomography(fromNpArray, toNpArray)[0]

    def transform(self, vector : tuple[int, int]) -> tuple[np.float32, np.float32]:
        """
        Transforms a vector by a matrix
        """
        rotatedVector = np.array(vector, np.float32).reshape(-1, 1, 2)
        transformedVector =  cv2.perspectiveTransform(rotatedVector, self._tMatrix)[0][0]
        return (transformedVector[1], transformedVector[0])
    
    def inverseTransform(self, vector : tuple[int, int]) -> tuple[np.float32, np.float32]:
        """
        Transforms a vector by a matrix
        """
        rotatedVector = np.array(vector, np.float32).reshape(-1, 1, 2)
        transformedVector =  cv2.perspectiveTransform(rotatedVector, self._inverseTMatrix)[0][0]
        return (transformedVector[1], transformedVector[0])
    
    def transform4Mediapipes(self, coords : tuple[int, int]) -> tuple[float, float]:
        """
        Converts a coordinate in projector space to a coordinate in camera space
        stored as a float from 0 to 1 as done by mediapipes
        """
        cameraCoords = self.transform(coords)
        print(cameraCoords)
        print(self._cameraWidth, self._cameraHeight)
        print((cameraCoords[0]/self._cameraWidth , cameraCoords[1]/self._cameraHeight))
        return (cameraCoords[1]/self._cameraHeight, cameraCoords[0]/self._cameraWidth)

#HOTSPOT AND BUTTON CLASS

#TODO rename button class? the name is still slightly confusing
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

    def __init__(self, hotspot_id: int, projectorPos : Tuple[int, int], calibrator : Calibrator, event_listener: EventListener, radius: float = 0.08):
        self.id: int = hotspot_id
        self._onProjectorCoord: int = projectorPos
        self._onCameraCoord =  calibrator.transform4Mediapipes(projectorPos)
        self._radius: float = radius

        self._event_listener: EventListener = event_listener
        self._button: _Button = _Button()

        self._is_activated: bool = False
        self._time_activated: float = 0



    def draw(self, img) -> None:
        img_width = img.shape[1]
        img_height = img.shape[0]

        # Draw outer ring
        cv2.circle(img, (self._onProjectorCoord), self._onscreen_radius(), (255),
                   thickness=2)

        # Draw inner circle
        if self._button.get_pressed_amount() != 0:
            cv2.circle(img, self._onProjectorCoord, self._inner_radius(), (255),
                       thickness=-1)


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
        squared_dist = (self._onCameraCoord[0] - point.x) ** 2 + (self._onCameraCoord[1] - point.y) ** 2
        return squared_dist <= self._radius ** 2

    def activate(self) -> None:
        self._event_listener.OnPressDetected(self.id)
        self._time_activated = time.time()
        self._is_activated = True

    def deactivate(self) -> None:
        self._is_activated = False

#MAIN LOOP
#hand tracking and hotspot interaction

def normalizeToCamera(coord : Tuple[float, float], width : int, height : int) -> Tuple[float, float]:
    """
    Takes in a coordinate returned by mediapipes given as two values between 0 and 1 for x and y
    and returns the corresponding coordinate on the camera. 
    """
    return (coord.x*width, coord.y*height)


def run2(screenDimensions :Tuple[int, int], hotspots : List[Hotspot], calibrator) -> None:  # This function is called by Program.cs
    """
    Captures video and runs the hand-detection model to handle the hotspots.

    :param event_listener: Callbacks for communicating events back to the C# side.
    """

    

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

    logging.info("Initialisation done.")

    # basic opencv + mediapipe stuff from https://www.youtube.com/watch?v=v-ebX04SNYM
    cv2.namedWindow("Hotspots", cv2.WINDOW_FULLSCREEN)

    #TODO: use capture class to take photos on another thread
    while video_capture.isOpened():
        success, video_capture_img = video_capture.read()
        if not success:
            logging.warning("Unsuccessful video read; ignoring frame.")
            continue

        camera_height, camera_width, _ = video_capture_img.shape

        # run model
        video_capture_img_rgb = cv2.cvtColor(video_capture_img, cv2.COLOR_BGR2RGB)  # convert to RGB
        model_output = hands_model.process(video_capture_img_rgb)

        image=np.full(screenDimensions, 0,np.uint8) #generate empty background
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

        
            # #draw fingertips
            # index_coords = [normalizeToCamera(landmarks.landmark[8], camera_width, camera_height) for landmarks in
            #                     model_output.multi_hand_landmarks]
            # #transform coords with matrix and draw circle
            # for index_coord in index_coords:
            #     print([landmarks.landmark[8]  for landmarks in
            #                     model_output.multi_hand_landmarks])
            #     index_coord = calibrator.inverseTransform(index_coord)
            #     index_coord = (int(index_coord[0]), int(index_coord[1]))
            #     cv2.circle(image, index_coord, 3, 255, 2)

        
        # draw hotspot
        for hotspot in hotspots:
            hotspot.draw(image)

        cv2.imshow("Hotspots", image)

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



if __name__ == "__main__":
    class MyEventListener(EventListener):
        def OnPressDetected(self, hotspot_id):
            print(f"Hotspot {hotspot_id} activated.")
    eventLister = MyEventListener()
    
    SKIP_CALIBRATION = False

    calibrator = Calibrator((1080, 1920))
    if SKIP_CALIBRATION:
        calibrator.skipCalibration()
    else:
        calibrator.calibrate()
        if calibrator._tMatrix is None:
            exit()

    hotspots = [Hotspot(0, (700, 700), calibrator, eventLister), Hotspot(1, (100, 100), calibrator, eventLister)]

    run2((1080, 1920), hotspots, calibrator)