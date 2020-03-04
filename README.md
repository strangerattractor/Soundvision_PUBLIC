# The development environment
All developers have to use the following version of software
- Unity 2019.3.3f1
https://unity3d.com/de/get-unity/download/archive
- Microsoft Visual Studio Commmunity 2017. version 15.9.13 
https://docs.microsoft.com/en-gb/visualstudio/releasenotes/vs2017-relnotes-history
- Pure Data 0.49.1  **64 bit** (Install via installer) 
https://puredata.info/downloads/pure-data/releases/0.49-1
- Kinect Azure Sensor SDK 1.3.0 
https://docs.microsoft.com/en-us/azure/kinect-dk/sensor-sdk-download
- Kinect Azure Body Tracking SDK 0.9.5
https://www.microsoft.com/en-us/download/details.aspx?id=100636

# Recommended IDE
- rider by jetbrains if available
- Visual Studio 2017 + ReSharper

# How to properly clone the repo
After you clone the repo. please execute

``` git submodule update --init --recursive ```

This will recursive clone all submodules from the github.

## Running Pd and Unity Editor / Build
The unity project contains an Pd analyzer patch under Soundvision/UnityProject/Assets/StreamingAssets/pd/patch/analyzer.pd
always start the analyzer patch before running anything in the editor or as a build.

## shmem
The Unity project access the Arrays in Pd patch via shmem (Shmem) object

## Nuget package
Some advanced feature such as Kinect Azure requires extra nuget package, using Unity NuGet.

https://github.com/xoofx/UnityNuGet

To add a registry, edit SoundVision/UnityProject/Packages/manifest.json
