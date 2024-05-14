---
title: Installation
layout: default
permalink: /docs/installation/
has_children: false
nav_order: 2
---

# Installation
{: .no_toc }

Wall Projections currently has support for Windows, macOS (x64 only), and Linux (x64 and ARM64).

{: .note-title}
> Recommended
>
> Windows is the recommended platform to use to run *Wall Projections*, due to
> the ease of installation.

## Table of contents
{: .no_toc .text-delta }

1. TOC
{:toc}

## Download the app

1. Go to the [releases page](https://github.com/miloszwasacz/wall-projections/releases)
   and download the latest version of Wall Projections for your platform.
2. Extract the downloaded *.zip* file to a memorable folder, such as the Desktop.

## Install the app

### Windows

You are now ready to run the app, but there are some additional optional steps
that make it easier to run the app in the future.

{: .note-title}
> Note
> 
> When running the app for the first time, Windows may display a warning about
> running an app from an unknown publisher. Click **More info** and 
> then **Run** to proceed.

#### Create a shortcut

1. Inside the new *WallProjections* folder, find **WallProjections.exe**.
2. Right-click the **WallProjections.exe** file and click **Create shortcut**.
3. Move the shortcut to a location of your choice, such as the Desktop.
4. The app can now be run by double-clicking the shortcut.

#### Run Wall Projections automatically at startup

1. Press **Windows Key + R**, and type *shell:startup*. This opens the **Startup** folder.
2. Copy and paste the shortcut you created from the desktop to the **Startup** folder.
3. The app should now run automatically when you start your computer _(a restart may be required)_.

<!--TODO macOS -->

### Ubuntu/Debian

1. Open a terminal window.
2. Navigate to the directory where you extracted the *.zip* file.
3. Run the following command to make the app executable:
   ```bash
   sudo chmod -R u+rwx WallProjections
   ```
4. Navigate to the *WallProjections* folder and run the installer script:
    ```bash
    sudo bash ./install.sh
    ```
5. The app can now be run using:
   ```bash
   ./WallProjections
   ```

### Other Linux distributions

1. Install the required dependencies using you package manager: `python3.11`, `python3.11-venv`, `vlc`, `libvlc-dev`
2. Navigate to the directory where you extracted the *.zip* file.
3. Give the app executable permissions:
   ```bash
   sudo chmod -R u+rwx WallProjections
   ```
4. Create a Python virtual environment:
   ```bash
   python3.11 -m venv "$HOME/.config/WallProjections/VirtualEnv"
   ```
5. Install the required Python packages:
   ```bash
   # Assuming that you are in the directory where you extracted the *.zip* file
   ~/.config/WallProjections/VirtualEnv/bin/python -m pip install -r ./WallProjections/Scripts/requirements.txt
   ```
6. The app can now be run using:
   ```bash
   # Assuming that you are in the directory where you extracted the *.zip* file
   ./WallProjections/WallProjections
   ```

## Get your content ready!

You're all set up with *Wall Projections*! Create or import some content to begin.

{: .note-title}
> Troubleshooting
> 
> If you have any issues, please refer to the [troubleshooting guide]({%link docs/usage/troubleshooting.md %}).
