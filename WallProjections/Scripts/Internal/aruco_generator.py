import os.path
from cv2 import aruco, imwrite

DICT = aruco.getPredefinedDictionary(aruco.DICT_7X7_100)
"""The CV2 ArUco dictionary, from which we generate our images. 
Defined here https://docs.opencv.org/3.4/d9/d6a/group__aruco.html#gac84398a9ed9dd01306592dd616c2c975."""

ARUCO_COUNT = 100
"""The number of ArUcos we generate. Must not exceed the size of DICT
(DICT_7x7_100 has max size 100, DICT_4x4_50 has max size 50, etc)."""

ARUCO_SIZE = 100
"""The size of each ArUco image in pixels"""

ARUCO_BORDER = 1
"""The size of each ArUco border in "squares"."""

FOLDER = os.path.abspath(__file__ + "/../../../Assets/ArUcos")
"""The path to the folder where ArUco images get stored"""


def get_aruco_path(aruco_id):
    return FOLDER + os.path.normpath("/ArUco_" + str(aruco_id) + ".png")


def generate_arucos():
    """Generates ArUco pngs (for use in calibration) in FOLDER if they don't already exist"""

    # Check if any ArUco images missing from FOLDER
    arucos_missing = False
    for code in range(ARUCO_COUNT):
        file = get_aruco_path(code)
        if not (os.path.exists(file) and os.path.isfile(file)):
            arucos_missing = True
            break

    # If any ArUco images are missing generate all of them
    if arucos_missing:
        os.makedirs(FOLDER, exist_ok=True)
        for code in range(ARUCO_COUNT):
            aruco_image = aruco.generateImageMarker(DICT, code, ARUCO_SIZE, borderBits=ARUCO_BORDER)  # fetch ArUco
            imwrite(get_aruco_path(code), aruco_image)
        print("Generated " + str(ARUCO_COUNT) + " ArUcos")
    else:
        print("All ArUcos already exist")


if __name__ == "__main__":
    generate_arucos()
