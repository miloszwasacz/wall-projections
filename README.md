# 2023-WallProjections
[![.NET CI](https://github.com/spe-uob/2023-WallProjections/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/spe-uob/2023-WallProjections/actions/workflows/dotnet-ci.yml)
[![GitHub Pages Deployment](https://github.com/spe-uob/2023-WallProjections/actions/workflows/github-pages.yml/badge.svg?branch=docs)](https://github.com/spe-uob/2023-WallProjections/actions/workflows/github-pages.yml)

> [!Important]
> Find the complete documentation here: [spe-uob.github.io/2023-WallProjections/](https://spe-uob.github.io/2023-WallProjections/)

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
  * [More Info](#more-info)
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
Raspberry Pi Model 4.

## Getting Started
The software can be installed on Windows, MacOS, and Linux computers, as well as on the Raspberry Pi for deployment.

> Go to installation guide: [spe-uob.github.io/2023-WallProjections/docs/installation/](https://spe-uob.github.io/2023-WallProjections/docs/installation/)


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

- As a **museum visitor**, I want to interact with the installation, so that I can learn about the artefacts in an engaging way.
- As a **museum visitor**, I want to make full use of the system without any prior tutorial or teaching, so that as much time as possible is spent learning about the artefacts.
- As an **exhibit designer**, I want to integrate the system with the rest of the exhibition, so that it does not look out of place.
- As an **exhibit designer**, I want to edit the location of hotspots with a graphical interface, so that it is inutitive and takes less time.
- As an **exhibit designer**, I want to edit hotspot information with a graphical interface, so that it is easier to line up the hotspots with the artefacts.
- As an **exhibit designer**, I want to package the data, so that the museum curator can load it up without manual editing.
- As a **museum curator**, I want to quickly setup the installation, so that it is ready to use with minimal effort.
- As a **museum curator**, I want to be able to easily diagonose any issues with the system, so that it has minimal downtime.
- As a **museum curator**, I want to spend as little money as possible maintaining the system, so that more money can be spent.

## Releases

### MVP

#### Hotspot Input
- [x] Buttons are used to activate hotspots instead of a computer vision based hand tracking system.
- [x] Pressing of a button sends an event to the information display.

#### Hotspot Display
- **N/A**

#### Information Display
- [x] Only a single image or video can be displayed at once.
- [x] Styling is left very minimal with just the information displayed.

#### Editor
- [x] Configuration for hotspot information is created manually through the creation of a zip file containing a `config.json` file and media files.
- [x] The configuration is manully loaded from the zip file on every opening of the program.

### First Release

#### Hotspot Input (Computer Vision)
- [x] Computer vision is used to both calibrate the projector and camera to match.
- [x] Hand tracking is used to tell when someone is touching a hotspot.
- [ ] All of the computer vision code runs at a good speed on a Raspberry Pi Model 4B.
- [ ] The computer vision updates the hotspot and information displays.

#### Hotspot Display
- [ ] Hotspots are projected onto the artefacts.
- [x] Animations are added to show users that a hotspot has been activated.

#### Information Display
- [x] The hotspot data is retained between closing and reopening the software.
- [ ] Styling is added to make the information display more visually appealing and engaging.
- [ ] Multiple images and videos can be displayed at the same time, while keeping the content easily readable and digestible.
- [ ] Animatons/transitions are used to make the software feel less static.
- [ ] Automatic layouting is used to allow for a more complex and appealing visual presentation.

#### Editor
- [x] An editor can be used to add text, images, and video to hotspots.
- [ ] The editor has support to easily reposition hotspot locations relative to the artefact.
- [x] Any changes the editor makes can be saved to the disk.

### Final Release

#### Hotspot Input (Computer Vision)
- [ ] Crop the camera view to reduce the required processing power for hand tracking.
- [ ] Use a faster model to quickly find hands, before using the slower model to find a fingertip on a much smaller area of the image.

#### Hotspot Display
- [ ] Hotspots illuminate one at a time when a user enters the space of the installation.
- [ ] Hotspots have a rich, engaging animation making it obvious when it is activated.

#### Information Display
- [ ] Transitions upon activation of a hotspot will be eye catching.

#### Editor
- [ ] A preview shows what the information will look like.

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
- [Python 3.11](https://www.python.org/downloads/release/python-3110/)
- [Python.NET](https://github.com/pythonnet/pythonnet)
- [MediaPipe](https://developers.google.com/mediapipe)

## More Info
- [User Flows](docs/USER_FLOWS.md)
- [Useful Links](docs/USEFUL_LINKS.md)
- [Main SEP Repository](https://github.com/spe-uob/SEP2023)

## Group Members

| Member | Account                                            |
|--------|----------------------------------------------------|
| Sergi  | [@Rolodophone](https://github.com/Rolodophone)     |
| Daniel | [@danieldiamand](https://github.com/danieldiamand) |
| Sonny  | [@ed22699](https://github.com/ed22699)             |
| Miłosz | [@miloszwasacz](https://github.com/miloszwasacz)   |
| Thomas | [@LW22736](https://github.com/LW22736)             |
