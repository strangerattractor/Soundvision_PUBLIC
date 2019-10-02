# The development environment
All developers have to use the following version of software
- Unity 2018.4.3f1 (LTS)
- Microsoft Visual Studio Commmunity 2017. version 15.9.13
- Pure Data 0.49.1  **64 bit** (Install via installer)

# Recommended IDE
- rider by jetbrains if available
- Visual Studio 2017 + ReSharper

# Build Pool
Access my [NAS](http://quickconnect.to)  
QuickConnectID: gyudon  
Name: cylvester  
Pass: dortmund  

# How to properly clone the repo
After you clone the repo. please execute

``` git submodule update --init --recursive ```

This will recursive clone all submodules from the github.

## PdBackend

The unity project contains a pd binary under StreamingAssets folder
PdBackend.cs monobehaviour automatically launches the Pd process when the game is started.
To use this Features PdBackend should exist in the scene.

## shmem
The Unity project access the Arrays in Pd patch via shmem (Shmem) object

## wix heat command
execute following command under bin directory

heat dir . -ag -gg -dr Cylvester -directoryid Cylvester -srd -sreg -cg UnityComponentGroup -out source.wxs -var var.UnityBuildDir

