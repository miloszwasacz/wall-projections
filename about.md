---
layout: default
title: About
permalink: /about/
---

A project in collaboration with Bristol Museums, to allow 3-D printed replicas of artefacts to be interacted with to 
display detailed, rich information about the touched area.

The functionality includes a camera and 2 projectors. The first projector overlays hotspots (glowing circles) onto the 
3D printed replica of the artefact. The camera positioned next to this projector uses computer vision to detect when a 
user's hand touches a hotspot, and upon a delay, activates the hotspot. A second project pointed against a wall will 
display information pertinent to the selected hotspot. This information is displayed in a rich, multimedia format, 
including text, images, and video.

The system aims to be intuitive and require no prior knowledge to use, with at most only simple instructions required to 
make full use of the system.

The system features an editor to allow for full visual editing of hotspots, with no editing of config files or manual 
import of media required. The configuration can be backed up via a single, easy to manage file and later restored back 
to into the program.

## Technologies

### Hardware/OS
- [Raspberry Pi Model 4B](https://www.raspberrypi.com/products/raspberry-pi-4-model-b/)
- [Raspberry Pi OS with desktop](https://www.raspberrypi.com/software/raspberry-pi-desktop/)

### User Interface
- [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- [Avalonia UI](https://docs.avaloniaui.net/)
- [LibVLCSharp](https://code.videolan.org/videolan/LibVLCSharp)
- [NUnit](https://docs.nunit.org/)

### Computer Vision
- **TODO:** Get Python version
- [Python.NET](https://github.com/pythonnet/pythonnet)
- [MediaPipe](https://developers.google.com/mediapipe)

## Members

| Member | Account                                            |
|--------|----------------------------------------------------|
| Sergi  | [@Rolodophone](https://github.com/Rolodophone)     |
| Daniel | [@danieldiamand](https://github.com/danieldiamand) |
| Sonny  | [@ed22699](https://github.com/ed22699)             |
| Miłosz | [@miloszwasacz](https://github.com/miloszwasacz)   |
| Thomas | [@LW22736](https://github.com/LW22736)             |
