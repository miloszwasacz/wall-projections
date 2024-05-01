---
title: Usage
layout: default
permalink: /docs/usage/
has_children: true
nav_order: 3
---

# Usage
{: .no_toc }

Here are some instructions on how to use Wall Projections.

## Table of contents
{: .no_toc .text-delta }

- TOC
{:toc}

## Starting the app

To start Wall Projections, simply run the executable file you have set up during the
[installation process]({%link docs/installation/index.md %}).

## Interacting with hotspots

If you have created or imported a configuration, you will see the main screen of the app.
If not, you will be prompted to create a new configuration using the [Editor](#configuring-the-artifact-display).

### Main screen

This screen shows the information about the artifact. It is dynamically updated when you interact with the hotspots.

{: .note-title}
> Key shortcuts
>
> There are some important key shortcuts to remember:
> - `Esc` closes the app
> - `F` toggles fullscreen mode
> - `E` opens the [Editor](#configuring-the-artifact-display)

### Hotspot display

The hotspots are displayed on the screen as circles. To interact with them, simply touch the hotspot with your
fingertips. Once the hotspot is fully activated, the relevant information will be displayed on the main screen.

{: .note-title}
> Fullscreen mode
>
> To toggle fullscreen mode, press `F` *(the hotspot display window must be focused)*.

## Configuring the artifact display

The Editor allows you to create or import configurations for the exhibit.
The list on the left lets you add, delete, and select hotspots to configure.

### Editing information

The middle section of the Editor allows you to edit the title and description of the currently selected hotspot.
This can be done manually or by importing a text file.

{: .note-title}
> Text file format
>
> The first line of the text file is the title, and the rest is the description.

This section also includes a button to set the location of the hotspot. Move your mouse to the desired location
on the hotspot display window and click to set the location, or press `Esc` to cancel.

### Managing media

The right section of the Editor allows you to manage the media (images and videos) associated with the hotspot.
Each media item also has a button to open the file explorer at its location, 
so that it can be viewed using an external application.

### Calibrating the camera

The toolbar at the top of the Editor includes a button to calibrate the camera.
This is necessary for the hand tracking to correctly identify the location of the hotspots.
Once clicked, the hotspot display window will show a grid of markers, 
and you will be prompted to position it correctly - ensure that the window is on the correct screen (projector) 
and is in fullscreen mode - then press **Continue**.

### Importing and exporting configurations

The toolbar at the top of the Editor includes buttons to import and export configurations (under **File**).

{: .note-title}
> Notes
> 
> Importing a configuration will overwrite the current configuration.
> This means that any data currently stored by the application will be lost.
> 
> To export a configuration, it must be saved first. 
