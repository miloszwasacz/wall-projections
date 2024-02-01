"""
Function drawArucoFrame and findArucoMarker based on
https://dvic.devinci.fr/tutorial/how-to-automatically-calibrate-a-camera-projector-system

"""

import numpy as np
import cv2
from cv2 import aruco

projected_coords = [[450,500],[450,700], [700,700], [1300, 550], [1350,700] ,[200, 200]]

def get_frame():

    cap = cv2.VideoCapture(cv2.CAP_ANY)
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 1920)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 1080)
    
    ok, frame = cap.read()

    cap.release()
    
    if not ok:
        cv2.destroyAllWindows()
        print("No camera detected")
        input()
        exit()
    
    return frame


#given ArUco dictionary and an id to coordinate dictionary, will draw the ArUco's at corrisponding coords.
def drawAurco(arucoDict, idCoordDict):
    background=np.full((1080,1920), 255,np.uint8) #generate empty background
    for key in idCoordDict:
        arucoImage = aruco.generateImageMarker(arucoDict, key, 100, borderBits= 1) #fetch ArUco
        topLeftCoord = idCoordDict[key]
        #replace pixels in background with ArUco image
        background[topLeftCoord[0]:topLeftCoord[0]+arucoImage.shape[0], topLeftCoord[1]:topLeftCoord[1]+arucoImage.shape[1]] = arucoImage
    return background

#temp function that generates an id to coordinate dictionary for a 6x12 grid
def genIdCoordDict():
    idCoordDict = {}
    k = 0
    for i in range(6):
        for j  in range(12):
            idCoordDict[k] = (25+(150*i), 25+(150*j))
            k = k+1 
    return idCoordDict


#
def findArucoMarkers(img, arucoDict, displayResults = False):
    arucoParam = aruco.DetectorParameters()
    corners, ids, _ = aruco.detectMarkers(img, arucoDict, parameters=arucoParam)
    idCoordDict = {}
    if(ids is None):
        print("0 aruco patterns detected, at least 4 required for calibration")
    elif(ids.size < 4):
        print("%s aruco patterns detected, at least 4 required for calibration" % ids.size)
    else:
        print("Success! %s aruco patterns detected" % ids.size)
        for i in range(ids.size):
            idCoordDict[ids[i][0]] = (int(corners[i][0][0][0]), int(corners[i][0][0][1]))
    
    if displayResults == True:
        cv2.imshow('Identified Aruco Markers', aruco.drawDetectedMarkers(frame, corners, ids))
        cv2.waitKey(5000)
    return idCoordDict



projectedIdCoordDict = genIdCoordDict()
arucoDict = aruco.getPredefinedDictionary(aruco.DICT_7X7_100)

cv2.imshow('Pool',drawAurco(arucoDict, projectedIdCoordDict))
cv2.waitKey(800)

frame = get_frame()
camereaIdCoordDict = findArucoMarkers(frame, arucoDict)




for key in projectedIdCoordDict:
    if key in camereaIdCoordDict:
        print(key, "projected coords:", projectedIdCoordDict[key], ",camera coords:", camereaIdCoordDict[key])
    else:
        print(key, "projected coords:", projectedIdCoordDict[key], ", camerea coords: NOT FOUND")