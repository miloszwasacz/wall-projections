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

def drawArucoFrame():
    #place aruco patters on images at projected_coords coordinates
    arucoFrame=np.full((1080,1920,3), 255,np.uint8)
    aruco0 = cv2.imread("WallProjections/Scripts/aruco0.png")
    aruco1 = cv2.imread("WallProjections/Scripts/aruco1.png")
    aruco2 = cv2.imread("WallProjections/Scripts/aruco2.png")
    aruco3 = cv2.imread("WallProjections/Scripts/aruco3.png")
    aruco4 = cv2.imread("WallProjections/Scripts/aruco4.png")
    aruco5 = cv2.imread("WallProjections/Scripts/aruco5.png")

    arucoFrame[projected_coords[0][1]:projected_coords[0][1]+aruco0.shape[0], projected_coords[0][0]:projected_coords[0][0]+aruco0.shape[1]] = aruco0
    arucoFrame[projected_coords[1][1]:projected_coords[1][1]+aruco1.shape[0], projected_coords[1][0]:projected_coords[1][0]+aruco1.shape[1]] = aruco1
    arucoFrame[projected_coords[2][1]:projected_coords[2][1]+aruco2.shape[0], projected_coords[2][0]:projected_coords[2][0]+aruco2.shape[1]] = aruco2
    arucoFrame[projected_coords[3][1]:projected_coords[3][1]+aruco3.shape[0], projected_coords[3][0]:projected_coords[3][0]+aruco3.shape[1]] = aruco3
    arucoFrame[projected_coords[4][1]:projected_coords[4][1]+aruco4.shape[0], projected_coords[4][0]:projected_coords[4][0]+aruco4.shape[1]] = aruco4
    arucoFrame[projected_coords[5][1]:projected_coords[5][1]+aruco5.shape[0], projected_coords[5][0]:projected_coords[5][0]+aruco5.shape[1]] = aruco5

    return arucoFrame 

def findArucoMarkers(img, markerSize=4, totalMarkers=250,draw=True):
    imgGray = cv2.cvtColor(img,cv2.COLOR_BGR2GRAY)
    key = getattr(aruco,f'DICT_{markerSize}X{markerSize}_{totalMarkers}')
    arucoDict = aruco.getPredefinedDictionary(key)
    arucoParam = aruco.DetectorParameters()
    bboxs, ids, _ = aruco.detectMarkers(imgGray, arucoDict, parameters=arucoParam)
    # print(ids,bboxs)
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
    return coords

background = drawArucoFrame()
cv2.imshow('Pool',background)
cv2.waitKey(800)

frame = get_frame()
coords = findArucoMarkers(frame)
print(coords)