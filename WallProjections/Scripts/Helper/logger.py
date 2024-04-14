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
        # Unknown
        logging.error("Unsupported OS for logging, storing in Scripts")
        return ''


def setup_logger(log_name, level=logging.INFO) -> logging.Logger:
    """
    Sets up a logger that stores logs in log folder with the format log_name_date_time.log. Should only be called
    at entry points.
    Returns the setup logger.
    """
    date_time = datetime.datetime.now().strftime("_%Y-%m-%d_%H-%M-%S.log")
    folder_path = _get_folder_path()

    if not os.path.exists(folder_path):
        logging.info(f"Creating folder {folder_path}")
        os.makedirs(folder_path)

    logging.basicConfig(filename=os.path.join(folder_path, log_name + date_time),
                        filemode='a',
                        format='%(asctime)s, %(filename)s, %(levelname)s: %(message)s',
                        datefmt='%H:%M:%S',
                        level=level)

    return logging.getLogger("logger")


def get_logger() -> logging.Logger:
    """Returns a previous setup logger, or default logger otherwise. Should only be called at non-entry points."""
    return logging.getLogger('logger')
