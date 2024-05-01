---
title: Development
layout: default
permalink: /docs/development/
has_children: false
nav_order: 3
---

# Development
{: .no_toc }

This section is for developers who are interested in contributing to Wall Projections.

## Table of contents
{: .no_toc .text-delta }

- TOC
{:toc}

## Prerequisites

Wall Projections is split into two main subsystems: **.Net** frontend and **Python** backend.
However, the frontend build system still requires some Python dependencies to run,
so it is recommended to install all the prerequisites for both systems.

### Frontend

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet)
- An IDE with .NET support
  (e.g. [JetBrains Rider](https://www.jetbrains.com/rider/), [Visual Studio](https://visualstudio.microsoft.com/))
- _(Linux only)_ VLC and LibVLC
  ```shell
  # Example for Ubuntu/Debian
  sudo apt-get install vlc libvlc-dev
  ```

{: .note-title}
> Note
>
> If you are developing on macOS for ARM64, you need to install the .NET 6.0 SDK for x64,
> as some dependencies are not available for ARM64.

### Backend

- [Python 3.11](https://www.python.org/downloads/)
- An IDE with Python support
  (e.g. [PyCharm](https://www.jetbrains.com/pycharm/), [Visual Studio Code](https://code.visualstudio.com/))

## Initial Setup

### Cloning the repository

First, clone the repository from [GitHub](https://github.com/spe-uob/2023-WallProjections).

```shell
git clone git@github.com:spe-uob/2023-WallProjections.git
```

{: .note-title}
> Note
>
> From now on, all commands are assumed to be run from the project root directory, unless otherwise specified.

### Installing dependencies

#### Frontend

If you are using an IDE, it should automatically restore the dependencies for you.

If you are using the terminal, you can restore the dependencies by running:

```shell
dotnet restore
```

#### Backend (Windows)

This step is only required if you are only developing the Python backend.
If you are developing the frontend, the Python dependencies will be installed as part of the frontend build process.

We recommend using a virtual environment to manage the Python dependencies.
It should be created in the `%APPDATA%\WallProjections\VirtualEnv` folder
using the following command:

```shell
# Make sure that you are using Python 3.11
python -m venv %APPDATA%\WallProjections\VirtualEnv
```

Then, install all the necessary Python dependencies:

```shell
# Replace \path\to\WallProjections with the actual path to the Wall Projections repository
pushd %APPDATA%\WallProjections\VirtualEnv\Scripts
.\python -m pip install -r \path\to\WallProjections\WallProjections\Scripts\Test\requirements.txt
popd
```

#### Backend (Linux/macOS)

We recommend using a virtual environment to manage the Python dependencies.
It should be created in the `~/.config/WallProjections/VirtualEnv` directory
using the following command:

```shell
python3.11 -m venv ~/.config/WallProjections/VirtualEnv
```

Then, install all the necessary Python dependencies:

```shell
~/.config/WallProjections/VirtualEnv/bin/python -m pip install -r ./WallProjections/Scripts/Test/requirements.txt
```

## Building and running

The frontend has 3 build configurations: `Debug`, `DebugSkipPython` and `Release`.

{: .note-title}
> _DebugSkipPython_ configuration
> 
> `DebugSkipPython` should be used when you only want to build the frontend without running the Python backend -
the program will use a mock backend instead.
> 
> Note that the build process still runs some Python scripts, even when using this configuration.

If you are not using an IDE, you can build and run the frontend using this command:
```shell
# Use the configuration you want to build
cd WallProjections
dotnet run -c Debug 
```

## Running the tests

### Frontend

The tests should be run using the `DebugSkipPython` configuration.

If you are not using an IDE, you can run the tests using this command:
```shell
dotnet test -c DebugSkipPython --verbosity normal
```
or to test with coverage:
```shell
dotnet test -c DebugSkipPython --collect:"XPlat Code Coverage" --settings coverlet.runsettings --results-directory TestResults
```

### Backend

To run the backend tests, use the following command:
```shell
# Remember to use the virtual environment instead of the system Python
python -m unittest discover -s ./WallProjections
```