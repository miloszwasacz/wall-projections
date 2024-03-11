import logging

# noinspection PyPackages
from .Helper.Calibrator import Calibrator
# noinspection PyPackages
from .Interop.json_dict_converters import json_to_2dict
# noinspection PyPackages
from .Interop import numpy_dotnet_converters as npnet


def calibrate(projector_id_to_coord_json: str):
    projector_id_to_coord = json_to_2dict(projector_id_to_coord_json)
    logging.info("Deserialized: " + str(projector_id_to_coord))
    return npnet.asNetArray(Calibrator.calibrate(projector_id_to_coord))
