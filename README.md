# PiDropLapse

This is a small .NET 5 console project designed to run on a Raspberry Pi that takes a photo, writes the date onto the picture, saves the photo locally and uploads the photo to Dropbox. Scheduling via cron on the Raspberry Pi allows you to create a series of photos that you can view remotely and share via Dropbox. This was created for learning, fun and to spy on our beloved Cockatiels... (Their consent was not obtained, I suspect when they learn about this project this repo will get taken down...)

There are many, many, existing cameras/software/hardware solutions for home surveillance or time-lapse creation that already exist and can do better/faster/more things... What this project is about:
 - Only stills, no video, to increase the chances of reasonable viewing on poor celluar internet connections
 - Low/No Cost Remote access without storing photos on servers/behind software that I don't trust and don't want to pay a subscription to and without setting up a server myself
 - Simple sharing of the photos to non-technical users
 - Learning and creating rather than buying!

With those goals in mind it seemed like a simple custom C# program would do the job nicely:
 - Runs on a Raspberry Pi with Camera - relatively inexpensive, compact, easy to find information about and generally fun and interesting
 - Pushes the photos to Dropbox for easy remote viewing - Dropbox has apps on all major platforms, an easy system to control access, a free tier and a large market share so there is a chance someone you want to share with already uses the platform.
 - Simple to setup and run via cron to avoid re-creating scheduling software

Downsides:
 - Current solution is specific to the Raspberry Pi and only tested by me on a 4b
 - You will need to setup an 'app' in Dropbox and store an access token in plain text on the Pi

Features:
 - Ini File allows setting some camera settings like Shutter Speed, ISO, Exposure Compensation, ...
 - Files are prefixed with a number that decreases every hour (well until the end of the day on 12/31/9999 anyway - no support will be provided after that date...) - this causes files from the most recent hour to be at the top of the file list in the default Dropbox sort order - if you have spent time in long Dropbox file lists I suspect that you may  appreciate this small detail...
 - Very basic help on the command line
 - Can use -WriteIni on the command line to generate a new ini file with the default settings
 - The Date and Time the photo is taken is drawn into the picture for easy reference (the size of the Date and Time is scaled based on the width of the picture)

## Used By and In Building PiDropLapse
Tools:
 - [Visual Studio IDE](https://visualstudio.microsoft.com/), [.NET Core (Linux, macOS, and Windows)](https://dotnet.microsoft.com/download/dotnet-core)
 - [ReSharper: The Visual Studio Extension for .NET Developers by JetBrains](https://www.jetbrains.com/resharper/)
 - [LINQPad - The .NET Programmer's Playground](https://www.linqpad.net/)

Hardware:
 - [Raspberry Pi 4 Model B](https://www.raspberrypi.org/products/raspberry-pi-4-model-b/)
 - [Raspberry Pi Camera Module V2](https://www.raspberrypi.org/products/camera-module-v2/)

Packages/Libraries/Services:
 - [dropbox/dropbox-sdk-dotnet: The Official Dropbox API V2 SDK for .NET](https://github.com/dropbox/dropbox-sdk-dotnet)
 - [techyian/MMALSharp: C# wrapper to Broadcom's MMAL with an API to the Raspberry Pi camera.](https://github.com/techyian/MMALSharp)
 - [thomasgalliker/ObjectDumper: ObjectDumper is a utility which aims to serialize C# objects to string for debugging and logging purposes.](https://github.com/thomasgalliker/ObjectDumper)
 - [cemdervis/SharpConfig: An easy to use CFG/INI configuration library for .NET.](https://github.com/cemdervis/SharpConfig)
