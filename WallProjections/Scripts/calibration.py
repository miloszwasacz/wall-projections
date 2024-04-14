import logging
import datetime
# noinspection PyPackages
from .Helper.Calibrator import Calibrator
# noinspection PyPackages
from .Interop import numpy_dotnet_converters as npnet
# noinspection PyPackages
from .Interop.json_dict_converters import json_to_2dict
# noinspection PyPackages
from .Helper.logger import setup_logger

logger = setup_logger("calibration")


def calibrate(projector_id_to_coord_json: str):
    logging.info("Calibrating...")
    projector_id_to_coord = json_to_2dict(projector_id_to_coord_json)
    transform_matrix = npnet.asNetArray(Calibrator.calibrate(projector_id_to_coord))
    logging.info("Calibration complete.")
    return transform_matrix

