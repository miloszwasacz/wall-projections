import logging
import datetime
import os


def _get_folder_path() -> str:
    if os.name == 'posix':
        # Unix/Linux/MacOS
        return os.path.join(os.path.expanduser('~'), '.config', 'WallProjections', 'Logs')
    elif os.name == 'nt':
        # Windows
        return os.path.join(os.getenv('APPDATA'), 'WallProjections', 'Logs')
    else:
        # Unsupported
        logging.error("Unsupported OS for logging, storing in Scripts/Logs")
        if os.path.basename(os.getcwd()) == "Scripts":
            scripts_path = os.getcwd()
        else:
            scripts_path = os.path.dirname(os.getcwd())  # navigates up one level from current working dir
        path = os.path.join(scripts_path, 'Logs')
        return path


def setup_logger(log_name, level=logging.INFO) -> logging.Logger:
    """
    Sets up a logger that stores logs in log folder with the format log_name_date_time.log. Should only be called
    at entry points.
    Returns the setup logger.
    """
    date = datetime.datetime.now().strftime("%Y-%m-%d")
    folder_path = _get_folder_path()

    if not os.path.exists(folder_path):
        logging.info(f"Creating folder {folder_path}")
        os.makedirs(folder_path)

    file_handler = logging.FileHandler(os.path.join(folder_path, log_name + '_' + date + '.log'))
    file_handler.setLevel(level)

    formatter = logging.Formatter('%(asctime)s, %(filename)s, %(levelname)s: %(message)s')
    file_handler.setFormatter(formatter)

    logger = logging.getLogger("logger")
    logger.setLevel(level)
    logger.addHandler(file_handler)
    logger.propagate = False
    return logger


def get_logger() -> logging.Logger:
    """Returns a previous setup logger, or default logger otherwise. Should only be called at non-entry points."""
    return logging.getLogger('logger')



