import logging
import time

logging.basicConfig(level=logging.INFO)
detection_running = False


# noinspection PyMethodMayBeStatic
class EventHandler:
    def OnHotspotPressed(self, hotspot_id):
        logging.info("hotspot pressed " + str(hotspot_id))

    def OnHotspotUnpressed(self, hotspot_id):
        logging.info("hotspot unpressed " + str(hotspot_id))


def hotspot_detection(event_handler: EventHandler) -> None:
    """
    Given hotspot projector coords, a transformation matrix and an event_handler
    calls events when hotspots are pressed or unpressed
    """

    global detection_running
    detection_running = True

    for i in range(30):
        event_handler.OnHotspotPressed(i % 3)
        time.sleep(1)
        event_handler.OnHotspotUnpressed(i % 3)
        time.sleep(1)
        if not detection_running:
            break


def stop_hotspot_detection():
    logging.info("stopping hotspot detection")
    global detection_running
    detection_running = False
    pass
