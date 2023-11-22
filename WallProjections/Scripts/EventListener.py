from abc import ABC, abstractmethod


class EventListener(ABC):
    @abstractmethod
    def on_hotspot_activated(self, hotspot_id):
        pass
