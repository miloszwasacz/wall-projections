# 2023-WallProjections
[![.NET CI](https://github.com/spe-uob/2023-WallProjections/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/spe-uob/2023-WallProjections/actions/workflows/dotnet-ci.yml)

## Contents

<!-- TOC -->
* [2023-WallProjections](#2023-wallprojections)
  * [Contents](#contents)
  * [Introduction](#introduction)
  * [Getting Started](#getting-started)
  * [Stakeholders](#stakeholders)
    * [Bristol Museums](#bristol-museums)
    * [Curator](#curator)
    * [Exhibition Designer](#exhibition-designer)
    * [Museum Educator](#museum-educator)
    * [Museum Director](#museum-director)
  * [User stories](#user-stories)
  * [Releases](#releases)
    * [MVP](#mvp)
    * [First Release](#first-release)
    * [Final Release](#final-release)
  * [Technologies used](#technologies-used)
    * [Hardware/OS](#hardwareos)
    * [User Interface](#user-interface)
    * [Computer Vision](#computer-vision)
  * [User Flows](#user-flows)
  * [Useful Links](#useful-links)
  * [Group Members](#group-members)
<!-- TOC -->

## Introduction

A project in collaboration with Bristol Museums, to allow 3-D printed replicas of artefacts to be interacted with to 
display detailed, rich information about the touched area.

The functionality includes a camera and 2 projectors. The first projector overlays hotspots (glowing circles) onto the
3-D printed replica of the artefact. The camera positioned next to this projector uses computer vision to detect when a 
user's hand touches a hotspot, and upon a delay, activates the hotspot. A second project pointed against a wall will 
display information pertinent to the selected hotspot. This information is displayed in a rich, multimedia format, 
including text, images, and video.

The system aims to be intuitive and require no prior knowledge to use, with at most only simple instructions required to
make full use of the system.

The system features an editor to allow for full visual editing of hotspots, with no editing of config files or manual 
import of media required. The configuration can be backed up via a single, easy to manage file and later restored back 
to into the program. We aim for the software to be installed and ran on a small form factor computer, specifically the
RaspberryPi Model 4.

## Getting Started
The software can be installed on Windows, MacOS, and Linux computers, as well as on the Raspberry Pi for deployment.

> **TODO:** Add setup guides for all platforms for both deployment and development.


## Stakeholders

### Bristol Museums

- Requested and oversees development of the system. 
- Wants to make exhibits more engaging by increasing interactivity and improving information density for artifacts without making it overwhelming.
- Does not like that current systems are time consuming to setup and must be heavily customised by people with specialised skills.

### Curator

- Would like to create exhibits which are more engaging and as informative as possible.
- Oversees funding for exhibits so would like the system to be inexpensive to install and maintain.
- System should be attractive to board members so funding will be accepted to install. 

### Exhibition Designer

- Use system to make new exhibits which are inventive and engaging.
- System should be polished and easy to integrate with the rest of the exhibit.
- System should require little custom configuration so more time can be spent on other aspects of exhibit.

### Museum Educator

- Concerned with educating visitors about artefacts.
- Would like the system to be able to effectively educate visitors with a higher density of information than current solutions.

### Museum Director

- Would like to have exhibits that drive more visitors, making the museum more successful.
- System should make the museum feel innovative and up to date compared to other museums.

## User stories

- As a museum visitor, I want to interact with the installation, so that I can learn more about it.
- As an exhibit maintainer, I want to set up the installation, so that it is ready for visitors to use.
- As an exhibit maintainer, I want to be able to relocate the hotspots, so that they are in the correct place after moving the artifact or switching it for a different one.
- As an exhibit maintainer, I want to be able to calibrate the software, so that the finger tracking is accurate.
- As an exhibit maintainer, I want to be able to update the content that is projected, so that it reflects the information we want to convey.
- **TODO:** Add more/update user stories for more stakeholders.

## Releases

### MVP
Use buttons (instead of a camera system) to trigger the projection of hotspots (lit-up circles) onto the replica artifact.

### First Release

### Final Release

## Technologies used

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

## [User Flows](docs/USER_FLOWS.md)

## [Useful Links](docs/USEFUL_LINKS.md)

## Group Members

| Member | Account                                       |
|--------|-----------------------------------------------|
| Sergi  | [@Rolodophone](https://github.com/Rolodophone) |
|   Daniel     |    [@danieldiamand](https://github.com/danieldiamand)                   |
|      Sonny        |  [@ed22699](https://github.com/ed22699)                                                                       |
|     Miłosz              |  [@miloszwasacz](https://github.com/miloszwasacz)                                                                                                             |
|Thomas|[@LW22736](https://github.com/LW22736)|
