## Archived...

For several years my wife and I used this project and a Raspverry Pi 4b to monitor our birds - it worked really well! In recent years our needs have changed and we no longer make use of this setup so this project has been archived...

Sometime after we stopped using this I did create another Pi based photo project - [PiSlicedDayPhotos](https://github.com/cmiles/PiSlicedDayPhotos). PiSlicedDayPhotos targets taking periodic landscape/outdoor photos from a Pi and is in use as I write this.

This project was written for fun and for the love of programming! If you are here looking for something I hope you find a useful snippet, idea or piece of code - contact me if you have any questions - Charles Miles, 9/18/2023  

# PiDropLapse

This project is two .NET 5 console projects designed to collect information from a Raspberry Pi, save it locally and upload it to Dropbox:
 - PiDropPhoto: Takes a photo and can write the temperature and pressure from a BMP280 sensor onto the image.
 - PiDropSimpleSensorReport: Reads the temperature and pressure from a BMP280 sensor, writes the readings to a local SQLite db and   creates an image with the current reading and chart of recent readings.

## Why

This was created for learning, fun and to spy on our beloved Cockatiels... (Their consent was not obtained, I suspect when they learn about this they will try to swoop in and get this repo taken down...)

Obviously there are many existing cameras/software/hardware solutions for home surveillance, photography, sensor/weather monitoring, etc. that can do better/faster/more things... However there are a few interesting details to this solution:
 - Photos and information are static images only (no video, no javascript, nothing 'active') to increase the chances of reasonable viewing on poor cellular internet connections
 - Can run without any access to the internet
 - Runs on relatively inexpensive multi-purpose hardware that you have complete control over
 - Can upload to Dropbox to allow easy online access and simple sharing with good/easy controls


Downsides:
 - Current solution is specific to the Raspberry Pi and only tested by me on a 4b
 - Doesn't have alerts, motion detection, video or other advanced features you might find in any 'true' home surveillance/monitoring setup
 - You will need to setup a Dropbox 'app' and store the access token in plain text on the Pi to use Dropbox (go to https://www.dropbox.com/developers/ to setup an app and get a token)

Features:
 - Files are prefixed with a number that decreases every hour (until the end of the day on 12/31/9999 anyway - no support will be provided after that date...) - this causes files from the most recent hour to be at the top of the file list in the 'default' sort order. This hardly seems worth listing as a feature until you work daily with Dropbox folders that have very long file lists...
 - Very basic help on the command line
 - -WriteIni command line flag to generate a new ini file with the default settings
 - Ini Files to allow simple access to settings

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
 - [GitHub - dotnet/iot: This repo includes .NET Core implementations for various IoT boards, chips, displays and PCBs.](https://github.com/dotnet/iot)
 - [cemdervis/SharpConfig: An easy to use CFG/INI configuration library for .NET.](https://github.com/cemdervis/SharpConfig) 
 - [techyian/MMALSharp: C# wrapper to Broadcom's MMAL with an API to the Raspberry Pi camera.](https://github.com/techyian/MMALSharp)
 - [dropbox/dropbox-sdk-dotnet: The Official Dropbox API V2 SDK for .NET](https://github.com/dropbox/dropbox-sdk-dotnet)
 - [thomasgalliker/ObjectDumper: ObjectDumper is a utility which aims to serialize C# objects to string for debugging and logging purposes.](https://github.com/thomasgalliker/ObjectDumper)
 - [GitHub - riezebosch/crayon: 🖍 Paint your console from .NET](https://github.com/riezebosch/Crayon)
 - [GitHub - dotnet-ad/Microcharts: Create cross-platform (Xamarin, Windows, ...) simple charts.](https://github.com/dotnet-ad/Microcharts/)
 - [GitHub - dotnet/efcore: EF Core is a modern object-database mapper for .NET. It supports LINQ queries, change tracking, updates, and schema migrations.](https://github.com/dotnet/efcore)
 - [SQLite](https://www.sqlite.org/index.html)
 - [GitHub - sarathkcm/Pluralize.NET: Pluralize or singularize any English word.](https://github.com/sarathkcm/Pluralize.NET)
 - [GitHub - mono/SkiaSharp: SkiaSharp is a cross-platform 2D graphics API for .NET platforms based on Google's Skia Graphics Library. It provides a comprehensive 2D API that can be used across mobile, server and desktop models to render images.](https://github.com/mono/SkiaSharp)
 - [GitHub - toptensoftware/RichTextKit: Rich text rendering for SkiaSharp](https://github.com/toptensoftware/richtextkit)
 
