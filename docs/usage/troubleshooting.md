---
title: Troubleshooting
layout: default
permalink: /docs/usage/troubleshooting
parent: Usage
has_children: false
nav_order: 1
---

# Troubleshooting

Here are some common scenarios where Wall Projections may not work as expected, and how to resolve them.

{: .note-title}
> General troubleshooting tip
>
> The app logs of any errors that occur. The log files can be found in the `%APPDATA%\WallProjections\Logs` folder 
> on Windows, or `~/.config/WallProjections/Logs` on Linux and macOS.
>
> The app can also be run with extra console output by running it from the command line with the `--trace` flag.

## Hotspots not being activated

- When starting the app or after closing the Editor, it might take a few seconds for the hand tracking to start.
  Make sure to wait a few seconds before trying to touch the hotspots.
- Make sure the camera is connected and no other apps are using the camera.
- Check the camera is working and the hotspot area is in-frame by using any other camera app - just make sure to
  close the camera app before starting Wall Projections.
- For the users' hands to be tracked reliably, the room must be bright enough that the camera can make out the hands
  against the background. Try making the room brighter. It is recommended to have the room slightly dim so that the
  projector's light is more visible, however the room mustn't be too dim either.
- Try recalibrating to make sure the app knows where the projector is with respect to the camera.
- The app only tracks the location of the 5 fingertips, so make sure to use your fingertips (rather than e.g. the palm
  of your hand) to touch the hotspots.
- Some angles at which the camera sees your hand work better than others. It needs to be clear from the camera's view
  that your hand is a hand. Try moving your hand around or moving the camera around to see if that helps. It is
  recommended to have the camera located directly above the hotspot area, facing downwards.
- Please note that only a maximum of 4 hands can be tracked at once. If you have more than 4 hands in the camera's view,
  the app will only track the first 4 hands it sees.

## Calibration not working

- When starting the app with a configuration already loaded, it might take a few seconds for the hand tracking to start.
  This blocks the calibration process, so make sure that everything has been correctly initialized
  (usually indicated by a light on the camera - it has to be on, not flashing) before opening the Editor.
    - If the calibration gets blocked, restart the app and try waiting a little longer before opening the Editor.
- Make sure the camera is connected and no other apps are using the camera.
- Check the camera is working and the projection is in-frame by using any other camera app - just make sure to close the
  camera app before starting Wall Projections.
- For the calibration to work, the projected square patterns must be well exposed from the camera's view. This means
  the projector mustn't be too bright or too dim compared to the light level of the room.
- Try making the room brighter or darker, or increasing or decreasing the brightness of the projector.
