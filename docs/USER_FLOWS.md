# Flows

## Museum visitor - interact with hotspot

1. Approach the replica artefact.
2. Touch a hotspot on the artefact.
3. Information regarding that part of the artefact is projected onto a nearby wall.
4. Read/watch the information projected to learn more about the artefact.

## Curator - setup

1. Install the hardware (projector, computer & camera) high-up on the wall, facing the artifact.
2. Follow the "calibration" flow to calibrate the camera system.
3. Follow the "relocate hotspots" flow to position the hotspots in the correct place. (This would also have to include some way to add/remove hotspots.)
4. Follow the "update content" flow to add the content to be projected.

## Curator - calibration

Calibrating to ensure the camera hand detection lines up with the projected hotspots.

1. Activate the calibration procedure from the UI.
2. The software automatically projects tracking markers across the view, and determines the location of symbols using computer vision.
3. The software tells the curator that calibration is finished.

## Curator - relocate hotspots

1. Switch on hotspot relocation mode.
2. Adjust location of hotspots interactively (e.g. using a mouse).
3. Press the save button to save the new locations.

## Exhibit designer - update content

1. Open the WallProjections software on a laptop/PC.
2. Add all the required content for all the hotspots (images, text, video etc.)
3. Export the project with all the added content to a USB drive.
4. Plug the USB drive, as well as a mouse and keyboard to the setup Raspberry Pi.
5. Open the editor on the WallProjections software on the Raspberry Pi and import the project from the USB drive.
6. Move the hotspot locations to the desired positions on the artefacts.
7. Press the save button to save the updated positions.
