# PiDropLapse

This is a small .NET 5 console project designed to run on a Raspberry Pi that takes a photo, writes the date (and optionally the temperature and pressure) onto the picture, saves the photo locally and can upload the photo to Dropbox.

## Why

This was created for learning, fun and to spy on our beloved Cockatiels... (Their consent was not obtained, I suspect when they learn about this they will try to swoop in and get this repo taken down...)

Obviously there are many existing cameras/software/hardware solutions for home surveillance, time-lapse creation and photography that can do better/faster/more things... However there are a few interesting details to this solution:
 - Only stills, no video, to increase the chances of reasonable viewing on poor cellular internet connections
 - Can run without any access to the internet
 - Runs on relatively inexpensive multi-purpose hardware that you have complete control over
 - Can upload to Dropbox to allow easy online access and simple sharing with good/easy controls


Downsides:
 - Current solution is specific to the Raspberry Pi and only tested by me on a 4b
 - Doesn't have alerts, motion detection, video or other advanced features you might find in any 'true' home surveillance/timelapse setup
 - You will need to setup a Dropbox 'app' and store the access token in plain text on the Pi to use Dropbox (go to https://www.dropbox.com/developers/ to setup an app and get a token)

Features:
 - Ini File allows setting some camera settings like Shutter Speed, ISO, Exposure Compensation, ...
 - Files are prefixed with a number that decreases every hour (until the end of the day on 12/31/9999 anyway - no support will be provided after that date...) - this causes files from the most recent hour to be at the top of the file list in most default sort orders. This works very nice without the default on Dropbox...
 - Very basic help on the command line
 - -WriteIni command line flag to generate a new ini file with the default settings
 - The Date and Time the photo is taken is written onto the photos for easy reference
 - Can use a BMP280 sensor (via I2C) to write Temperature and Pressure onto the photos

Install Requirements:
 - In addition to a stock Raspian install with camera enabled 'sudo apt-get install libgdiplus' is required.

## Used By and In Building PiDropLapse

Tools:
 - [.NET Core (Linux, macOS, and Windows)](https://dotnet.microsoft.com/download/dotnet-core)
 - [Visual Studio IDE](https://visualstudio.microsoft.com/)
 - [ReSharper: The Visual Studio Extension for .NET Developers by JetBrains](https://www.jetbrains.com/resharper/)
 - [LINQPad - The .NET Programmer's Playground](https://www.linqpad.net/)

Hardware:
 - [Raspberry Pi 4 Model B](https://www.raspberrypi.org/products/raspberry-pi-4-model-b/)
 - [Raspberry Pi Camera Module V2](https://www.raspberrypi.org/products/camera-module-v2/)
 - [Adafruit BMP280 I2C or SPI Barometric Pressure & Altitude Sensor](https://www.adafruit.com/product/2651)

Packages/Libraries/Services:
 - [dropbox/dropbox-sdk-dotnet: The Official Dropbox API V2 SDK for .NET](https://github.com/dropbox/dropbox-sdk-dotnet)
 - [techyian/MMALSharp: C# wrapper to Broadcom's MMAL with an API to the Raspberry Pi camera.](https://github.com/techyian/MMALSharp)
 - [GitHub - dotnet/iot: This repo includes .NET Core implementations for various IoT boards, chips, displays and PCBs.](https://github.com/dotnet/iot)
 - [thomasgalliker/ObjectDumper: ObjectDumper is a utility which aims to serialize C# objects to string for debugging and logging purposes.](https://github.com/thomasgalliker/ObjectDumper)
 - [cemdervis/SharpConfig: An easy to use CFG/INI configuration library for .NET.](https://github.com/cemdervis/SharpConfig)
 - [GitHub - riezebosch/crayon: 🖍 Paint your console from .NET](https://github.com/riezebosch/Crayon)

 
