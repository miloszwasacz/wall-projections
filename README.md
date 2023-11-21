# 2023-WallProjections

A project with Bristol Museum, to create interactive replicas of museum exhibits using projectors.

## Useful links

- [SEP projects list](https://www.ole.bris.ac.uk/bbcswebdav/pid-8046087-dt-content-rid-46849402_2/xid-46849402_2)
- Raspberry
  - [.NET app deployment for Raspberry](https://learn.microsoft.com/en-us/dotnet/iot/deployment)
  - [.NET GPIO tutorial for Raspberry](https://learn.microsoft.com/en-us/dotnet/iot/tutorials/gpio-input)
- Avalonia
  - [Avalonia docs](https://docs.avaloniaui.net/docs/next/welcome)
  - [LibVLCSharp docs](https://code.videolan.org/videolan/LibVLCSharp)
  - [Avalonia VideoView sample (LibVLCSharp)](https://code.videolan.org/videolan/LibVLCSharp/-/tree/3.x/samples/LibVLCSharp.Avalonia.Sample)
- NUnit
    - [NUnit docs](https://docs.nunit.org/)
    - [Avalonia NUnit sample](https://github.com/AvaloniaUI/Avalonia.Samples/tree/main/src/Avalonia.Samples/Testing/TestableApp.Headless.NUnit)
- [Python.NET docs](https://pythonnet.github.io/pythonnet/)
- [MediaPipe docs](https://mediapipe-studio.webapps.google.com/home)

## Roles

- Sergi ([@Rolodophone](https://github.com/Rolodophone)) - project structure and interface
- Daniel ([@danieldiamand](https://github.com/danieldiamand)) - client liaison
- Sonny ([@ed22699](https://github.com/ed22699)) - meetings secretary
- Miłosz ([@miloszwasacz](https://github.com/miloszwasacz)) - project manager
- Thomas ([@LW22736](https://github.com/LW22736)) - merge master

## Stakeholders

### Exhibit maintainer

- Maintaining the installation
- Make it easy enough for them to be able to maintain it, e.g. fixing errors in the projection content or calibrating the hotspots/cameras

### Museum visitors

- Using the installation
- Make it engaging and easy to use and obvious how to use it

### Exhibit curator/management

- The installation should inform and educate people on that subject area

### Museum management

- Should generally be a good experience so that people pay to come to the exhibit, or recommend it to friends

## User stories

- As a museum visitor, I want to be able to interact with the installation, so that I can learn more about it.
- As an exhibit maintainer, I want to set up the installation, so that it is ready for visitors to use.
- As an exhibit maintainer, I want to be able to relocate the hotspots, so that they are in the correct place after moving the artifact or switching it for a different one.
- As an exhibit maintainer, I want to be able to calibrate the software, so that the finger tracking is accurate.
- As an exhibit maintainer, I want to be able to update the content that is projected, so that it reflects the information we want to convey.

## Flows

### Museum visitor - interact with hotspot

1. They approach the replica artefact.
2. The visitor touches a hotspot on the artefact.
3. Information regarding that part of the artefact is projected onto a nearby wall.
4. The visitor reads/watches the information projected to learn more about the artefact.

### Exhibit maintainer - setup

1. Install the hardware (projector, computer & camera) high-up on the wall, facing the artifact.
2. Follow the "relocate hotspots" flow to position the hotspots in the correct place. (This would also have to include some way to add/remove hotspots.)
3. Follow the "calibration" flow to calibrate the camera system.
4. Follow the "update content" flow to add the content to be projected.

### Exhibit maintainer - relocate hotspots

1. Switch on hotspot relocation mode.
2. Adjust location of hotspots interactively somehow (e.g. using a mouse).
3. Press some button to save the new locations.

### Exhibit maintainer - calibration

(Calibrating the camera system with the location of the hotspots.)

1. Ensure the artifact is well-lit with white light. (So that the computer vision works properly.)
2. Activate the calibration procedure from the UI.
3. The software automatically projects some symbol in place of each hotspot, determines the location of each symbol using computer vision, and plays a beep sound to indicate that the calibration is complete.

### Exhibit maintainer - update content

(could use a USB stick connected to the raspberry pi or whatever to store the content)

1. Take the USB stick from whatever computing hardware we’re using
2. Connect it into their computer
3. They transfer the files they want to be projected, i.e. an image or a video file for each hotspot
   - Could work simply using directory structure - each hotspot has its own folder which requires there to be one and only one image or video file inside of it to be projected
   - That would make it nice and easy for the museum staff
   - Could also have a simple program for editing the information stored in a file. Maybe use a rebranded zip file similar to Word .docx to cleanly package the information.
4. Plug the USB back into the hardware

## MVP

Use buttons (instead of a camera system) to trigger the projection of hotspots (lit-up circles) onto the replica artifact.

## Technologies used

- [Raspberry Pi OS with desktop](https://www.raspberrypi.com/software/raspberry-pi-desktop/)
- [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- [Avalonia UI](https://docs.avaloniaui.net/)
- [LibVLCSharp](https://code.videolan.org/videolan/LibVLCSharp)
- [Python.NET](https://github.com/pythonnet/pythonnet)
- [NUnit](https://docs.nunit.org/)
