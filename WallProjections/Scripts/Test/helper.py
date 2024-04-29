import os


def get_asset(path: str) -> str:
    return str(os.path.join(os.path.dirname(__file__), "Assets", path))
