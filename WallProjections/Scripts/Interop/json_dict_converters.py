import json


def json_to_2dict(json_dict: str) -> dict[int, tuple[float, float]]:
    dict_temp: dict[str, tuple[float, float]] = json.loads(json_dict)
    return {int(k): v for k, v in dict_temp.items()}


def json_to_3dict(json_dict: str) -> dict[int, tuple[float, float, float]]:
    dict_temp: dict[str, tuple[float, float, float]] = json.loads(json_dict)
    return {int(k): v for k, v in dict_temp.items()}
