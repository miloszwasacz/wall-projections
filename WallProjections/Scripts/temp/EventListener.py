from abc import ABC, abstractmethod


class EventListener(ABC):
    @abstractmethod
    def OnPressDetected(self, hotspot_id: int) -> None:
        pass
