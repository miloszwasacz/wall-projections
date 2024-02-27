import os.path
from cv2 import aruco
import cv2

NUMBER = 100  # max ArUco ID to gen
DICT = aruco.getPredefinedDictionary(aruco.DICT_7X7_100)  # ArUco dict used
ARUCO_SIZE = 100  # size of each ArUco in pixels
ARUCO_BORDER = 1  # size of the border in "squares"
FOLDER = os.path.abspath(__file__ + "/../../../Assets/ArUcos")  # folder for generated ArUcos


def get_aruco_path(aruco_id):
    return FOLDER + os.path.normpath("/ArUco_" + str(aruco_id) + ".png")


# Check if there are any ArUcos missing
all_exist = True
for code in range(NUMBER):
    file = get_aruco_path(code)
    if not (os.path.exists(file) and os.path.isfile(file)):
        all_exist = False
        break

if not all_exist:
    os.makedirs(FOLDER, exist_ok=True)
    for code in range(NUMBER):
        arucoImage = aruco.generateImageMarker(DICT, code, ARUCO_SIZE, borderBits=ARUCO_BORDER)  # fetch ArUco
        cv2.imwrite(get_aruco_path(code), arucoImage)
    print("Generated " + str(NUMBER) + " ArUcos")

else:
    print("All ArUcos already exist")
