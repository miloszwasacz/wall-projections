import cv2
import numpy as np

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
        cv2.setWindowProperty("Hotspots", cv2.WND_PROP_TOPMOST, 1)
        cv2.moveWindow("Hotspots", 1920, -20)  # Move rightwards to second monitor and upwards to hide top bar
        cv2.imshow("Hotspots", self._drawArucos(projectedCoords, arucoDict))
        cv2.waitKey(3000)
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

    def proj_to_cam(self, proj_coords : tuple[int, int]) -> tuple[np.float32, np.float32]:
        """
        Transforms a coordinate in projector space to a coordinate in camera space
        """
        rotatedVector = np.array(proj_coords, np.float32).reshape(-1, 1, 2)
        transformedVector =  cv2.perspectiveTransform(rotatedVector, self._tMatrix)[0][0]
        return (transformedVector[1], transformedVector[0])

    def proj_to_norm(self, proj_coords : tuple[int, int]) -> tuple[float, float]:
        """
        Transforms a coordinate in projector space to a coordinate in camera space
        then normalizes to floats between 0 and 1, as used by mediapipe
        """
        cameraCoords = self.proj_to_cam(proj_coords)
        return (cameraCoords[1]/self._cameraHeight, cameraCoords[0]/self._cameraWidth)
    
    def cam_to_proj(self, cam_coords : tuple[float, float]) -> tuple[int, int]:
        """
        Transforms a coordinate in camera space to a projector in camera space
        """
        rotatedVector = np.array(cam_coords, np.float32).reshape(-1, 1, 2)
        transformedVector =  cv2.perspectiveTransform(rotatedVector, self._inverseTMatrix)[0][0]
        return (int(transformedVector[1]), int(transformedVector[0]))
    
    def norm_to_proj(self, norm_coords : tuple[float, float]) -> tuple[int, int]:
        """
        Transforms a coordinate in camera space to a projector in camera space
        """
        cam_coords = (norm_coords[0]*self._cameraWidth, norm_coords[1]*self._cameraHeight)
        return self.cam_to_proj(cam_coords)
