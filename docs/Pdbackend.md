# How Pd Backend works

Sound Vision System requires audio input and sound analysis and we want to use Pd for that. However, it is cumbersome to deal with two software in the workflow. Our strategy is to keep Pd patch as simple as possible, and pull as many logical, compositional, and artistic components to Unity world.

## Pd without GUI

Pd can be launched without GUI from the command line (in Windows cmd.exe). It means if we can invoke the command from Unity software we can use Pd as a background process. In order to realize this **PdBackend.cs** exists. We need one and only one instance of PdBackend.cs in our scene so that the script starts the Pd process without GUI when the game starts and stops the Pd process when the game ends. 

## Pd Process and DSP

As soon as Pd process starts, it automatically starts DSP. There is currently no option to stop DSP.

## Where is Pd binary

In the Assets/StreamingAssets/pd/win folder. Unity serializes all data outside of StreamingAssets folder but files in this folder remain unarchived after build.

## Where is the main Pd patch

The patch stays in Assets/StreamingAssets/pd/patch folder. The name of patch is **analyzer.pd**

## Communication between Pd and Unity

via IPC (Inter Process Communication). I made an external Pd object called **shmem.dll** for this. shmem is an abbreviation of shared memory. Shared memory is a special region of RAM allocated by the OS that can be accessed by more than one processes (in our case Pd and Unity). You need to copy the entire data in Array object to shmem by sending bang to shmem object so that Unity can read the content of the designated array.  I'm using windows API directly thus no compatibility with Mac.

## Caution

Do not open the Pd patch and starts the game at the same time. If you do so, the behaviour is undefined.

