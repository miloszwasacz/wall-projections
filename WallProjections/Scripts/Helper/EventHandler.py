# noinspection PyMethodMayBeStatic
class EventHandler:
    def OnHotspotPressed(self, hotspot_id):
        print("hotspot pressed " + str(hotspot_id))

    def OnHotspotUnpressed(self, hotspot_id):
        print("hotspot unpressed " + str(hotspot_id))
