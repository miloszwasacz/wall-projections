from cv2 import aruco
import cv2

NUMBER = 100 #max aruco ID to gen
DICT = aruco.getPredefinedDictionary(aruco.DICT_7X7_100) #ArUco dict used
ARUCO_SIZE =100 #size of each aruco in pixels
ARUCO_BORDER = 1 #size of the border in "squares"

for code in range(NUMBER):
    arucoImage = aruco.generateImageMarker(DICT, code, ARUCO_SIZE, borderBits= ARUCO_BORDER) #fetch ArUco
    cv2.imwrite("./WallProjections/Scripts/ArUcos/ArUco_"+str(code)+".png", arucoImage)