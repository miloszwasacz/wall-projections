---
title: Windows
layout: default
permalink: /docs/installation/windows
parent: Installation
---

# Installation - Windows

## 1. Setting Up Python

### 1.1. Installing Python
Install the latest version of **Python 3.11** from: 
[https://www.python.org/downloads/](https://www.python.org/downloads/).

{: .important }
> This is not the latest version. Scroll until you see *Looking for a specific release?* and find the version
> starting with *Python 3.11.*

### 1.2. Setting Up Virtual Environment

To set up the virtual environment used by Python, open **Command Prompt** and type:
```
python -m venv %AppData%\WallProjections\VirtualEnv
```
Wait for this to complete. Then install the required dependencies:
```
%AppData%\WallProjections\VirtualEnv\Scripts\pip install "mediapipe==0.10.9" numpy opencv-python
```
This may take a while due to installing from the internet.

## 2. Get latest WallProjections
Download the [latest version of WallProjections](https://github.com/spe-uob/2023-WallProjections/releases/download/latest/WallProjections-win-x64.zip)
Extract the downloaded *.zip* file to a memorable folder. 

Inside the new *WallProjections* folder, find **WallProjections.exe**. 
Open **Wallprojections.exe** to open the *WallProjections* app.