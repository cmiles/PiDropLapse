﻿#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MMALSharp;
using MMALSharp.Common;
using MMALSharp.Common.Utility;
using MMALSharp.Handlers;
using PiDropPhoto;
using PiDropUtility;
using SharpConfig;
using SkiaSharp;
using Topten.RichTextKit;
using static Crayon.Output;

//
// Help Block
//

//Simple parsing of command line for help in the args
if (args.Length > 0 && args.Any(x => x.ToLower().Contains("help")))
{
    Console.WriteLine(Green("PiDropPhoto Help"));
    Console.WriteLine("When you execute PiDropPhoto it will attempt to:");
    Console.WriteLine(" - Take a photo and save it in a sub directory called 'Drops'");
    Console.WriteLine(" - Upload the File to Dropbox");

    Console.WriteLine(string.Empty);
    Console.WriteLine("Settings are pulled from PiDropPhoto.ini");
    Console.WriteLine(
        " - If the program doesn't find the ini file when in runs a new one will be generated");
    Console.WriteLine(" - Run PiDropPhoto -WriteIni to write out a new ini file");
    Console.WriteLine(" - For a Dropbox Upload to happen you need a valid AccessToken in the ini file");
    Console.WriteLine(" - Some Camera Settings can be adjusted in the ini file");
    Console.WriteLine(" - Set this up with cron to take a series of photos");

    Console.WriteLine(string.Empty);
    Console.WriteLine(
        "Created by Charles Miles - see https://github.com/cmiles/PiDropLapse for more information!");

    return;
}

//
// Ini Setup
//
var configFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, "PiDropPhoto.ini"));

//Simple parsing for writeini in the args
if (args.Length > 0 && args.Any(x => x.ToLower().Contains("writeini")))
{
    if (configFile.Exists)
    {
        Console.WriteLine(Red(
            $"Sorry - Won't overwrite an existing config - delete or rename {configFile.FullName} and then use -WriteIni again to create a fresh settings file."));
        Console.WriteLine();
        return;
    }

    Console.WriteLine($"Writing New Settings File to {Green(configFile.FullName)}...");
    Console.WriteLine();
    var setupConfig = new Configuration {Section.FromObject("Main", new PiDropPhotoSettings())};
    setupConfig.SaveToFile(configFile.FullName);
    return;
}

//
// Setup execution time and Photo Details
//
var executionTime = DateTime.Now;
var photoTextDetails = new List<string>();
photoTextDetails.Add(executionTime.ToString("yyyy-MM-dd HH:mm:ss"));
Console.WriteLine();
Console.WriteLine($"Starting PiDropPhoto Starting - {executionTime:yyyy-MM-dd-HH-mm-ss}");
Console.WriteLine();

//
// Process Ini File
//
Console.WriteLine($"Getting Config File - {Green(configFile.FullName)}");
if (!configFile.Exists)
{
    Console.WriteLine(Yellow("No Config File Found - writing Defaults"));
    var setupConfig = new Configuration {Section.FromObject("Main", new PiDropPhotoSettings())};
    setupConfig.SaveToFile(configFile.FullName);
}

Console.WriteLine("Loading Config File");
var configInformation = Configuration.LoadFromFile(configFile.FullName);
PiDropPhotoSettings config;
try
{
    Console.WriteLine("Parsing Config File");
    config = configInformation["Main"].ToObject<PiDropPhotoSettings>();
}
catch (Exception e)
{
    Console.WriteLine(Red($"Trouble Parsing Config - Using Defaults... Error {e}"));
    Console.WriteLine();
    config = new PiDropPhotoSettings();
}

Console.WriteLine("Config:");
Console.WriteLine(Cyan(ObjectDumper.Dump(config,
    new DumpOptions
        {ExcludeProperties = new List<string> {"DropboxAccessToken"}, DumpStyle = DumpStyle.Console})));
Console.WriteLine();

//
// Possible BMP280 Sensor
//
if (config.UseBmp280Sensor)
{
    Console.WriteLine("Trying BMP280 Temp and Pressure Sensor via I2C...");
    try
    {
        var (temperatureFahrenheit, pressureMillibars) = await Sensors.GetBmp280TemperatureAndPressure();

        if (temperatureFahrenheit != null || pressureMillibars != null)
            photoTextDetails.Add("BMP280");
        if (temperatureFahrenheit != null)
            photoTextDetails.Add($"{temperatureFahrenheit:0.#}\u00B0F");
        if (pressureMillibars != null)
            photoTextDetails.Add($"{pressureMillibars:0.#}mb");
    }
    catch (Exception e)
    {
        Console.WriteLine($"{Yellow("Trouble Getting BMP280 Readings")}{Environment.NewLine}{e}");
    }

    Console.WriteLine();
}

if (config.UseSi7021Sensor)
{
    Console.WriteLine("Trying Si7021 Temp and Humidity Sensor via I2C...");
    try
    {
        var (temperatureFahrenheit, humidityPercent) = Sensors.GetSiTemperatureAndHumidity();

        photoTextDetails.Add("Si7021");
        photoTextDetails.Add($"{temperatureFahrenheit:0.#}\u00B0F");
        photoTextDetails.Add($"rh {humidityPercent:0}%");
    }
    catch (Exception e)
    {
        Console.WriteLine($"{Yellow("Trouble Getting Si7021 Readings")}{Environment.NewLine}{e}");
    }

    Console.WriteLine();
}

//
// Setup Photo Directory and File
//
var photoTargetDirectory = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "Drops"));
Console.WriteLine($"Local photo directory: {photoTargetDirectory.FullName}");
if (!photoTargetDirectory.Exists) photoTargetDirectory.Create();
var photoFilePrefix = (DateTime.MaxValue - DateTime.Now).TotalHours.ToString("00000000");
Console.WriteLine($"Photo prefix {photoFilePrefix}");
var photoTargetFile = new FileInfo(Path.Combine(photoTargetDirectory.FullName,
    $"{photoFilePrefix}--{config.FileIdentifierName}--{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.jpg"));
if (photoTargetFile.Exists)
{
    Console.WriteLine(Yellow("Deleting Previous Photo"));
    photoTargetFile.Delete();
    photoTargetFile.Refresh();
}

//
// Camera Setup and Capture
//
Console.WriteLine("Initializing Camera");
MMALCamera piCamera = MMALCamera.Instance;
MMALCameraConfig.ISO = config.Iso;
MMALCameraConfig.ShutterSpeed = config.ExposureTimeInMicroSeconds;
MMALCameraConfig.Rotation = config.Rotation;
MMALCameraConfig.ExposureCompensation = config.ExposureCompensation;
if (MMALCameraConfig.Rotation == 0 || MMALCameraConfig.Rotation == 180)
    MMALCameraConfig.StillResolution = new Resolution(config.LongEdgeResolution,
        config.LongEdgeResolution / 16 * 9);
if (MMALCameraConfig.Rotation == 90 || MMALCameraConfig.Rotation == 270)
    MMALCameraConfig.StillResolution =
        new Resolution(config.LongEdgeResolution / 16 * 9, config.LongEdgeResolution);
using (var imgCaptureHandler = new ImageStreamCaptureHandler(photoTargetFile.FullName))
{
    piCamera.ConfigureCameraSettings(imgCaptureHandler);
    Console.WriteLine(Green("Taking Photo"));
    await piCamera.TakePicture(imgCaptureHandler, MMALEncoding.JPEG, MMALEncoding.I420);
}

Console.WriteLine("Cleaning Up");
piCamera.Cleanup();
Console.WriteLine();

//
// Write Information onto Photo
//
Console.WriteLine("Loading file to write date");
var photoBitmap = SKBitmap.Decode(photoTargetFile.FullName);
var photoCanvas = new SKCanvas(photoBitmap);
var width = photoBitmap.Width;
var height = photoBitmap.Height;
var maxHeight = width >= height ? height / 6 : height / 5;
var maxWidth = width - 50;
Console.WriteLine($"Photo Width {width}, Height {height}, Max Height {maxHeight}, Max Width {maxWidth}");
var photoText = string.Join(" - ", photoTextDetails);
Console.WriteLine($"Photo information text - {Green(photoText)}");
var adjustedFontSize =
    TextHelpers.AutoFitRichStringWidth(photoText, "Arial", maxWidth, maxHeight);
Console.WriteLine($"Adjusted Font Size - {adjustedFontSize}");
var titleRichString = new RichString()
    .FontFamily("Arial")
    .TextColor(SKColors.White)
    .FontSize(adjustedFontSize)
    .TextColor(SKColors.Red)
    .Add(photoText);
titleRichString.Paint(photoCanvas, new SKPoint(20, 40));
Console.WriteLine("Saving file with information written");
photoCanvas.Flush();
var finalPhoto = SKImage.FromBitmap(photoBitmap);
var data = finalPhoto.Encode(SKEncodedImageFormat.Jpeg, 100);
await using (var stream = new FileStream(photoTargetFile.FullName, FileMode.Create, FileAccess.Write))
{
    data.SaveTo(stream);
}

photoTargetFile.Refresh();
Console.WriteLine($"{Green(photoTargetFile.FullName)} - File Length {photoTargetFile.Length}");
Console.WriteLine();

//
// Dropbox Upload
//
if (string.IsNullOrWhiteSpace(config.DropboxAccessToken))
{
    Console.WriteLine("Did not find a DropboxAccessToken - skipping Dropbox processing and ending...");
    Console.WriteLine();
    return;
}

await DropboxHelpers.UploadFileToDropbox(photoTargetFile, config.DropboxAccessToken);