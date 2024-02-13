import abc


class EventListener(abc.ABC):
    @abc.abstractmethod
    def OnPressDetected(self, hotspot_id: int) -> None:
        pass
