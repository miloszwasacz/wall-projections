"""
ArUcos are n X n grids of black or white pixels with a black border, uniquely assigned an code in relation to an ArUco Dictionary
the method of calibration used here is inspired by both:
https://docs.opencv.org/4.x/d5/dae/tutorial_aruco_detection.html
https://github.com/GOSAI-DVIC/gosai/blob/master/core/calibration/calibration_auto.py
"""

import numpy as np
import cv2
from cv2 import aruco
from typing import Dict, Tuple



def takePhoto() -> np.ndarray:
    """
    Returns a photo from a detectable camera
    """
    cap = cv2.VideoCapture(cv2.CAP_ANY)
    cv2.waitKey(2000)
    ok, image = cap.read()

    cap.release()
    
    if not ok:
        cv2.destroyAllWindows()
        print("No camera detected")
        exit()
    
    return image

def drawArucos(projectedCoords : Dict[int, Tuple[int, int]], arucoDict : aruco.Dictionary) -> np.ndarray:
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

def detectArucos(img : np.ndarray, arucoDict : aruco.Dictionary, displayResults = False) -> Dict[int, Tuple[np.float32, np.float32]]:
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


def getTransformationMatrix(fromCoords : Dict[int, Tuple[np.float32, np.float32]], toCoords : Dict[int, Tuple[int, int]]) -> np.ndarray:
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


def transform(vector : Tuple[np.float32, np.float32], matrix : np.ndarray) -> Tuple[np.float32, np.float32]:
    """
    Transforms a vector by a matrix
    """
    rotatedVector = np.array(vector, np.float32).reshape(-1, 1, 2)
    transformedVector =  cv2.perspectiveTransform(rotatedVector, matrix)[0][0]
    return (transformedVector[1], transformedVector[0])


def projectDetectGetTransform(printData = False) -> np.ndarray:
    """
    Projects a grid of ArUcos, detects them and returns the transformation matrix
    """
    projectedCoords = {}
    code = 0
    for i in range(6):
        for j  in range(12):
            projectedCoords[code] = (25+(150*i), 25+(150*j))
            code = code+1 
    
    arucoDict = aruco.getPredefinedDictionary(aruco.DICT_7X7_100)

    cv2.namedWindow("Calibration", cv2.WINDOW_FULLSCREEN)

    cv2.imshow("Calibration", drawArucos(projectedCoords, arucoDict))
    cv2.waitKey(800)
    photo = takePhoto()

    cameraCoords = detectArucos(photo, arucoDict, displayResults=printData)
    
    camera2ProjectorMatrix = getTransformationMatrix(cameraCoords, projectedCoords)

    if printData:
        for iD in projectedCoords:
            if iD in cameraCoords:
                print("id:",iD,"|camera coord:", cameraCoords[iD], "| projected coord:", projectedCoords[iD], "| transformed: ", transform(cameraCoords[iD], camera2ProjectorMatrix))

    return camera2ProjectorMatrix

if __name__ == "__main__":
    projectDetectGetTransform(printData=True)