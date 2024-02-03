"""
ArUcos are n X n grids of black or white pixels with a black border, uniquely assigned an id in relation to an aruco Dictionary
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
    Returns a single photo from a detectable camera
    """
    cap = cv2.VideoCapture(cv2.CAP_ANY)
    ok, image = cap.read()

    cap.release()
    
    if not ok:
        cv2.destroyAllWindows()
        print("No camera detected")
        exit()
    
    return image

def drawArucos(projectedCoords : Dict[int, Tuple[int, int]], arucoDict : aruco.Dictionary) -> np.ndarray:
    """
    Takes in a dictionary of coordinates and of ArUcos, 
    and returns a white image with an ArUco drawn at each coordinate
    """
    image=np.full((1080,1920), 255,np.uint8) #generate empty background
    for iD in projectedCoords: #id is the name of a built in function in python, so use iD
        arucoImage = aruco.generateImageMarker(arucoDict, iD, 100, borderBits= 1) #fetch ArUco
        topLeftCorner = projectedCoords[iD]
        #replace pixels in background with ArUco image
        image[topLeftCorner[0]:topLeftCorner[0]+arucoImage.shape[0], topLeftCorner[1]:topLeftCorner[1]+arucoImage.shape[1]] = arucoImage
    return image

def detectArucos(img : np.ndarray, arucoDict : aruco.Dictionary, displayResults = False) -> Dict[int, Tuple[np.float32, np.float32]]:
    """
    Detects ArUcos in an image and returns a dictionary of ids to coords to the top left corner

    :param displayResults: displays the img with detected ArUcos highlighted

    """
    corners, ids, _ = aruco.detectMarkers(img, arucoDict, parameters=aruco.DetectorParameters())
    
    if displayResults == True:
        cv2.imshow("Labled Aruco Markers", aruco.drawDetectedMarkers(img, corners, ids))
        cv2.waitKey(3000)
    
    detectedCoords = {}
    if ids is None:
        return detectedCoords
    for i in range(ids.size):
        iD = ids[i][0]
        topLeftCorner = (corners[i][0][0][0], corners[i][0][0][1])
        detectedCoords[iD] = topLeftCorner
    return detectedCoords


def getTransformationMatrix(fromCoords : Dict[int, Tuple[int, int]], toCoords : Dict[int, Tuple[int, int]]) -> np.ndarray:
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

    return cv2.findHomography(fromNpArray, toNpArray)

if __name__ == "__main__":
    #generate an example projectCoords dictionary
    projectedCoords = {}
    k = 0
    for i in range(6):
        for j  in range(12):
            projectedCoords[k] = (25+(150*i), 25+(150*j))
            k = k+1 
    
    arucoDict = aruco.getPredefinedDictionary(aruco.DICT_7X7_100)

    cv2.namedWindow("Calibration", cv2.WINDOW_FULLSCREEN)

    cv2.imshow("Calibration", drawArucos(projectedCoords, arucoDict))
    cv2.waitKey(800)

    cameraCoords = detectArucos(takePhoto(), arucoDict, displayResults=True)

    camera2ProjectorMatrix = getTransformationMatrix(cameraCoords, projectedCoords)

    print(camera2ProjectorMatrix)
    #cv2.warpPerspective() to use matrix 