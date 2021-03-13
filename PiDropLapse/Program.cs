#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Dropbox.Api;
using Dropbox.Api.Files;
using MMALSharp;
using MMALSharp.Common;
using MMALSharp.Common.Utility;
using MMALSharp.Handlers;
using PiDropLapse;
using SharpConfig;
using static Crayon.Output;

//
// Help Block
//

//Simple parsing of command line for help in the args
if (args.Length > 0 && args.Any(x => x.ToLower().Contains("help")))
{
    Console.WriteLine(Green("PiDropLapse Help"));
    Console.WriteLine("When you execute PiDropLapse it will attempt to:");
    Console.WriteLine(" - Take a photo and save it in a sub directory called 'Drops'");
    Console.WriteLine(" - Upload the File to Dropbox");

    Console.WriteLine(string.Empty);
    Console.WriteLine("Settings are pulled from PiDropLapse.ini");
    Console.WriteLine(
        " - If the program doesn't find the ini file when in runs a new one will be generated");
    Console.WriteLine(" - Run PiDropLapse -WriteIni to write out a new ini file");
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
var configFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, "PiDropLapse.ini"));

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
    var setupConfig = new Configuration {Section.FromObject("Main", new PiDropLapseSettings())};
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
Console.WriteLine($"Starting PiDropLapse Starting - {executionTime:yyyy-MM-dd-HH-mm-ss}");
Console.WriteLine();

//
// Process Ini File
//
Console.WriteLine($"Getting Config File - {Green(configFile.FullName)}");
if (!configFile.Exists)
{
    Console.WriteLine(Yellow("No Config File Found - writing Defaults"));
    var setupConfig = new Configuration {Section.FromObject("Main", new PiDropLapseSettings())};
    setupConfig.SaveToFile(configFile.FullName);
}

Console.WriteLine("Loading Config File");
var configInformation = Configuration.LoadFromFile(configFile.FullName);
PiDropLapseSettings config;
try
{
    Console.WriteLine("Parsing Config File");
    config = configInformation["Main"].ToObject<PiDropLapseSettings>();
}
catch (Exception e)
{
    Console.WriteLine(Red($"Trouble Parsing Config - Using Defaults... Error {e}"));
    Console.WriteLine();
    config = new PiDropLapseSettings();
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
Image bmp;
await using (FileStream fs = new(photoTargetFile.FullName, FileMode.Open))
{
    bmp = Image.FromStream(fs);
    fs.Close();
}

photoTargetFile.Delete();

//Write the date into the top left of the photo using approximately 1/3
//of the width of the photo or a minimum of Verdana 12
var photoText = string.Join(" - ", photoTextDetails);
Console.WriteLine($"Writing information onto photo - {Green(photoText)}");
Graphics g = Graphics.FromImage(bmp);
var width = (int) g.VisibleClipBounds.Width;
Console.WriteLine($"Photo Width {width}");
var adjustedFont = ImageHelpers.TryAdjustFontSizeToFitWidth(g, photoText, new Font("Verdana", 12), width / 3, 12, 128);
Console.WriteLine($"Adjusted Font Size - {adjustedFont.Size}");
g.DrawString(photoText, adjustedFont, Brushes.Red, new PointF(20, 20));
Console.WriteLine("Saving file with information written");
bmp.Save(photoTargetFile.FullName);
photoTargetFile.Refresh();
Console.WriteLine($"{Green(photoTargetFile.FullName)} -- File Length {photoTargetFile.Length}");
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

Console.WriteLine($"Found a Dropbox Access Token - {Green("Opening Dropbox Connection")}");
DropboxClient dropClient;
try
{
    dropClient = new DropboxClient(config.DropboxAccessToken.Trim());
}
catch (Exception e)
{
    Console.WriteLine(Red($"Dropbox Exception:{Environment.NewLine}{e}"));
    return;
}

Console.WriteLine("Starting Dropbox Upload...");
var dropboxUploadResult =
    await dropClient.Files.UploadAsync(new CommitInfo($@"/{photoTargetFile.Name}"), photoTargetFile.OpenRead());
Console.WriteLine($"Dropbox File Uploaded (PiDropLapse App Directory){Green(dropboxUploadResult.PathDisplay)}");
Console.WriteLine();