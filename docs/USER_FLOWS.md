# Flows

## Museum visitor - interact with hotspot

1. They approach the replica artefact.
2. The visitor touches a hotspot on the artefact.
3. Information regarding that part of the artefact is projected onto a nearby wall.
4. The visitor reads/watches the information projected to learn more about the artefact.

## Exhibit maintainer - setup

1. Install the hardware (projector, computer & camera) high-up on the wall, facing the artifact.
2. Follow the "relocate hotspots" flow to position the hotspots in the correct place. (This would also have to include some way to add/remove hotspots.)
3. Follow the "calibration" flow to calibrate the camera system.
4. Follow the "update content" flow to add the content to be projected.

## Exhibit maintainer - relocate hotspots

1. Switch on hotspot relocation mode.
2. Adjust location of hotspots interactively somehow (e.g. using a mouse).
3. Press some button to save the new locations.

## Exhibit maintainer - calibration

(Calibrating the camera system with the location of the hotspots.)

1. Ensure the artifact is well-lit with white light. (So that the computer vision works properly.)
2. Activate the calibration procedure from the UI.
3. The software automatically projects some symbol in place of each hotspot, determines the location of each symbol using computer vision, and plays a beep sound to indicate that the calibration is complete.

## Exhibit maintainer - update content

(could use a USB stick connected to the raspberry pi or whatever to store the content)

1. Take the USB stick from whatever computing hardware we’re using
2. Connect it into their computer
3. They transfer the files they want to be projected, i.e. an image or a video file for each hotspot
    - Could work simply using directory structure - each hotspot has its own folder which requires there to be one and only one image or video file inside of it to be projected
    - That would make it nice and easy for the museum staff
    - Could also have a simple program for editing the information stored in a file. Maybe use a rebranded zip file similar to Word .docx to cleanly package the information.
4. Plug the USB back into the hardware
