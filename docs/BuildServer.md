# Build Server

# What is BUILD?
The goal of our project is to create a Windows BUILD. BUILD means a standalone application exported from the Unity Editor.
It runs much faster than the game mode in Unity Editor because it is optimized for performance not for debugging.

# Build Server
A small windows computer in the Chikashi's home. It runs 24/7 and checks SonudVision repository every 20 minutes for updates.
If the build server find any change (even one line of code) in the **master** branch of the SoundVision repo,
the server

1. clone the entire repo locally
2. build the software from scratch
3. run the unit tests
4. package the Sound Vision software in the installer
5. make a folder in the Chikashi's **NAS** with the build number
6. copy the installer in the folder

# NAS (Network Attached Storage)
In addition to the Builder Server, I have a **NAS** at home which can be accessed from everywhere.

The credentials are following
URL: http://gyudon.quickconnect.to
Name : cylvester
pass : dortmund

# Build number
The build server gives the build number automatically to the folder and we refer a specific version of software, using this number.
(e.g. the feature X is not working in Build 72 but fixed in 74)
 
# Duration of build
As written above, the build server checks the repo every 20 minutes and the build itself takes ca' 5 minutes so you can see a new folder in my NAS 30 minutes after push.






