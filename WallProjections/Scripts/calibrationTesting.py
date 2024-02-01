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

def drawAurco(aurcoDict, idCoordDict):
    arucoFrame=np.full((1080,1920), 255,np.uint8) #generate empty background
    for key in idCoordDict:
        arucoImage = aruco.generateImageMarker(aurcoDict, key, 100, borderBits= 1) 
        topLeftCoord = idCoordDict[key]
        arucoFrame[topLeftCoord[0]:topLeftCoord[0]+arucoImage.shape[0], topLeftCoord[1]:topLeftCoord[1]+arucoImage.shape[1]] = arucoImage
    return arucoFrame

def genIdCoordDict():
    idCoordDict = {}
    k = 0
    for i in range(6):
        for j  in range(12):
            idCoordDict[k] = (25+(150*i), 25+(150*j))
            k = k+1 
    return idCoordDict

def findArucoMarkers(img, markerSize=4, totalMarkers=250,draw=True):
    imgGray = cv2.cvtColor(img,cv2.COLOR_BGR2GRAY)
    key = getattr(aruco,f'DICT_{markerSize}X{markerSize}_{totalMarkers}')
    arucoDict = aruco.getPredefinedDictionary(key)
    arucoParam = aruco.DetectorParameters()
    bboxs, ids, _ = aruco.detectMarkers(imgGray, arucoDict, parameters=arucoParam)
    coords = []
    if(ids is None):
        print("0 aruco patterns detected, at least 4 required for calibration")
    if(ids.size < 4):
        print("%s aruco patterns detected, at least 4 required for calibration" % ids.size)
    else:
        print("Success! %s aruco patterns detected" % ids.size)
        #converting NumPy arrays into a int list + sort aruco patterns in order
        ids = [i[0] for i in ids.tolist()]
        coords = [bboxs[i][0][0].tolist() for i in range(len(ids))]
        coords = [[int(a),int(b)] for a,b in coords]
        sorted_pairs = sorted(zip(ids, coords))
        tuples = zip(*sorted_pairs)
        ids, coords = [ list(tuple) for tuple in  tuples]
        # print(ids)
    # print("coords : ", coords)
    return tuples

idCoordDict = genIdCoordDict()
aurcoDict = aruco.getPredefinedDictionary(aruco.DICT_7X7_100)

cv2.imshow('Pool',drawAurco(aurcoDict, idCoordDict))
cv2.waitKey(800)

frame = get_frame()
coords = findArucoMarkers(frame)
print(coords)