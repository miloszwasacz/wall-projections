---
title: Windows
layout: default
permalink: /docs/development/setup/windows/
parent: Development Setup
grand_parent: Development
nav_order: 1
---

# Development Setup - Windows

## 1. Install .Net 6
First, you need to install the development platform for *WallProjections*, **.Net 6**. To do this, open **PowerShell**
by searching in the **Start Menu** and type the command:

```
winget install Microsoft.DotNet.SDK.6
```

Click enter to run the command and install the SDK.

## 2. Get a .NET IDE
Ensure you have an IDE with .NET support to make development of *WallProjections* easier. The recommended IDE is 
**JetBrains Rider**, but any .NET IDE should work.

## 2. Clone the Repository
Then, clone the repository for *WallProjections* (currently hosted at 
[https://github.com/spe-uob/2023-WallProjections](https://github.com/spe-uob/2023-WallProjections)).
This can be done either using Git in the terminal, or by using the built in Git manager in Rider 
([Guide for Rider](https://www.jetbrains.com/help/rider/Set_up_a_Git_repository.html#clone-repo)). 

## 3. Restore Dependencies
Once cloned, open the repository inside your IDE and restore the dependencies, either by using the `dotnet restore` 
command in the terminal, or by using your IDE's built in nuget tools 
([Guide for Rider](https://www.jetbrains.com/help/rider/Using_NuGet.html#restoring)).