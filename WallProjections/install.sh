#!/bin/sh

# Install the required packages
apt-get install -y python3.11 python3.11-venv vlc libvlc-dev
echo "Packages installed"

# Add permissions to the folder
FOLDER=$(readlink -f "$0")
chmod -R u+rwx "$FOLDER"
echo "Permissions added"

# Create the virtual environment
VENV="$HOME/.config/WallProjections/VirtualEnv"
mkdir "$VENV"
python3.11 -m venv "$VENV"
echo "Python virtual environment created"

# Install the required packages
VENV_BIN="$VENV/bin"
"$VENV_BIN/python" -m pip install -r "$FOLDER/Scripts/requirements.txt"
echo "Python packages installed"

echo "Installation completed successfully!"
