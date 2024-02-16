---
title: Installation
layout: default
permalink: /docs/installation/
nav_order: 2
---

# Installation

WallProjections requires itself and python with the [MediaPipe](https://developers.google.com/mediapipe) library.

## Installing Python
The project uses **Python 3.11**.
Install from your package manager or from [www.python.org.](https://www.python.org/downloads/)

For the *WallProjections* software to run the hand tracking component, add the environment variable:
```shell
PYTHONDLL="PATH/TO/PYTHONDLL"
```

> This is normally similar to `python311.dll, python3.11.dylib, python3.11.so`

MediaPipe is used for hand tracking, so install it for Python in your terminal:
```shell
python -m pip install mediapipe
```

## Downloading WallProjections

- Download WallProjections for your platform from the [Releases](https://github.com/spe-uob/2023-WallProjections/releases) on the GitHub.
- Extract the folder from the `.zip` file to a memorable folder.

{: .note}
> On **MacOS**, always run the *x64* version, as the *ARM* version does not function.

## Run the Software
To run the software find and run the corresponding executable

### Windows
Double click `WallProjections.exe`.

> If you see a message saying that the software is unsafe, click **More Info** and **Run Anyway**.

### MacOS
Double click `WallProjections`.

{: .note-title}
> If you see a message saying ...*Unidentified Developer*
> - Go to: *System Settings*.
> - Go to: *Privacy and Security*.
> - Scroll and find: *"WallProjections" was blocked...*
> - Click *Run Anyway*

### Linux
Open a terminal in the *WallProjections* folder and type:
```shell
chmod u+x ./WallProjections
```

Then run the software with:
```shell
./WallProjections
```
