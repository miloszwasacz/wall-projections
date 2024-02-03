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

def drawArucos(id2Coords : Dict[int, Tuple[int, int]], id2Arucos : aruco.Dictionary) -> np.ndarray:
    """
    Takes in a dictionary of coordinates and of ArUcos, 
    and returns a white image with an ArUco drawn at each coordinate
    """
    image=np.full((1080,1920), 255,np.uint8) #generate empty background
    for key in id2Coords: #id is the name of a built in function in python, so used var name key.
        arucoImage = aruco.generateImageMarker(id2Arucos, key, 100, borderBits= 1) #fetch ArUco
        topLeftCoord = id2Coords[key]
        #replace pixels in background with ArUco image
        image[topLeftCoord[0]:topLeftCoord[0]+arucoImage.shape[0], topLeftCoord[1]:topLeftCoord[1]+arucoImage.shape[1]] = arucoImage
    return image


if __name__ == "__main__":
    #generate an id2Coord dictionary
    id2Coords = {}
    k = 0
    for i in range(6):
        for j  in range(12):
            id2Coords[k] = (25+(150*i), 25+(150*j))
            k = k+1 
    
    id2Arucos = aruco.getPredefinedDictionary(aruco.DICT_7X7_100)


    cv2.imshow("Calibration", drawArucos(id2Coords, id2Arucos))
    cv2.waitKey(800)