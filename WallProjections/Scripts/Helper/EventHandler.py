# noinspection PyMethodMayBeStatic
class EventHandler:
    def on_hotspot_pressed(self, hotspot_id):
        print("hotspot pressed " + str(hotspot_id))

    def on_hotspot_unpressed(self, hotspot_id):
        print("hotspot unpressed " + str(hotspot_id))
