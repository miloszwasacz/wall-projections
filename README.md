# 2023-WallProjections

A project with Bristol Museum, to create interactive replicas of museum exhibits using projectors.

## Useful links

[SEP projects list](https://www.ole.bris.ac.uk/bbcswebdav/pid-8046087-dt-content-rid-46849402_2/xid-46849402_2)

## Roles

- Sergi ([@Rolodophone](https://github.com/Rolodophone)) - project structure and interface
- Daniel ([@danieldiamand](https://github.com/danieldiamand)) - client liaison
- Sonny ([@ed22699](https://github.com/ed22699)) - meetings secretary
- Miłosz ([@miloszwasacz](https://github.com/miloszwasacz)) - project manager
- Thomas ([@LW22736](https://github.com/LW22736)) - merge master

## Stakeholders

### Museum staff

- Maintaining the installation
- Make it easy enough for them to be able to maintain it, e.g. fixing errors in the projection content or calibrating the hotspots/cameras

### Museum visitors

- Using the installation
- Make it engaging and easy to use and obvious how to use it

### Exhibit curator/management

- The installation should be inform and educate people on that subject area

### Museum management

- Should generally be a good experience so that people pay to come to the exhibit, or recommend it to friends

## User stories

### Museum visitor

1. They approach the replica artefact
2. The hotspots are projected onto the artefact?
3. The visitor touches a hotspot on the artefact.
4. Information regarding that part of the artefact is projected onto a nearby wall.
5. The visitor reads/watches the information projected to learn more about the artefact.

### Exhibit maintainer

Calibrating the camera system with the location of the hotspots:

1. Switch on some kind of calibration mode?
2. The hotspots are then projected
3. Maybe like put a brightly-coloured sticker on each of the hotspot locations
4. Press something to say that the stickers are in place
5. Some way to test the calibration was successful (although maybe you could just use the normal mode to test it)

Adjusting the location of the hotspots:

1. Switch on some kind of hotspot location mode
2. Put a brightly-coloured sticker on each of the desired locations for hotspots
3. Press something to say that the stickers are in place
4. The hotspots are projected onto the replica to check they’re in the right place (although maybe you could just use the normal mode to test it)

Updating hotspot content:

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
