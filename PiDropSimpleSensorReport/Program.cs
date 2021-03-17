using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PiDropPhoto;
using PiDropSimpleSensorReport;
using PiDropUtility;
using SharpConfig;
using SkiaSharp;
using static Crayon.Output;

//Simple parsing of command line for help in the args
if (args.Length > 0 && args.Any(x => x.ToLower().Contains("help")))
{
    Console.WriteLine(Green("PiDropSimpleSensorGraph Help"));
    Console.WriteLine("When you execute PiDropSimpleSensorGraph it will attempt to:");
    Console.WriteLine(" - Read sensor data and save it into a SQLite database named PiDropSimpleSensorReport.db");
    Console.WriteLine(" - Create an image with the current and recent readings");
    Console.WriteLine(" - (Optional) Upload the image to Dropbox");

    Console.WriteLine(string.Empty);
    Console.WriteLine("Settings are pulled from PiDropSimpleSensorReport.ini");
    Console.WriteLine(
        " - If the program doesn't find the ini file when in runs a new one will be generated");
    Console.WriteLine(" - Run PiDropPhoto -WriteIni to write out a new ini file");
    Console.WriteLine(" - For a Dropbox Upload to happen you need a valid AccessToken in the ini file");
    Console.WriteLine(" - Set this up with cron to get a series of readings");

    Console.WriteLine(string.Empty);
    Console.WriteLine(
        "Created by Charles Miles - see https://github.com/cmiles/PiDropLapse for more information!");

    return;
}

//
// Ini Setup
//
var configFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, "PiDropSimpleSensorReport.ini"));

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
    var setupConfig = new Configuration {Section.FromObject("Main", new PiDropSimpleSensorReportSettings())};
    setupConfig.SaveToFile(configFile.FullName);
    return;
}

//
// Process Ini File
//
Console.WriteLine($"Getting Config File - {Green(configFile.FullName)}");
if (!configFile.Exists)
{
    Console.WriteLine(Yellow("No Config File Found - writing Defaults"));
    var setupConfig = new Configuration {Section.FromObject("Main", new PiDropSimpleSensorReportSettings())};
    setupConfig.SaveToFile(configFile.FullName);
}

Console.WriteLine("Loading Config File");
var configInformation = Configuration.LoadFromFile(configFile.FullName);
PiDropSimpleSensorReportSettings config;
try
{
    Console.WriteLine("Parsing Config File");
    config = configInformation["Main"].ToObject<PiDropSimpleSensorReportSettings>();
}
catch (Exception e)
{
    Console.WriteLine(Red($"Trouble Parsing Config - Using Defaults... Error {e}"));
    Console.WriteLine();
    config = new PiDropSimpleSensorReportSettings();
}

Console.WriteLine("Config:");
Console.WriteLine(Cyan(ObjectDumper.Dump(config,
    new DumpOptions
        {ExcludeProperties = new List<string> {"DropboxAccessToken"}, DumpStyle = DumpStyle.Console})));
Console.WriteLine();

//
// Setup Database
//
var dbFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, "PiDropSimpleSensorReport.db"));
var dbContext = new SensorReadingContext(dbFile.FullName);
dbContext.Database.EnsureCreated();
if (!dbFile.Exists)
{
    Console.WriteLine(Red($"Database File {dbFile.FullName} Not Found?"));
    return;
}

Console.WriteLine($"Using Database File {Green(dbFile.FullName)}");
Console.WriteLine();

//
// Read Sensor Data and Write to DB
//
var readingDateTime = DateTime.Now;
var (temperatureFahrenheit, pressureMillibars) = await Sensors.GetBmp280TemperatureAndPressure();
if (temperatureFahrenheit != null)
{
    var dbReading = new SensorReading
    {
        ReadingDateTime = readingDateTime,
        ReadingValue = temperatureFahrenheit.Value,
        ReadingTag = "Temperature in F"
    };

    dbContext.SensorReadings.Add(dbReading);
    await dbContext.SaveChangesAsync();
    Console.WriteLine(
        $"Added Temperature Reading of {Green($"{dbReading.ReadingValue:0.#}\u00B0F")} to {dbFile.FullName}");
    Console.WriteLine(Cyan(ObjectDumper.Dump(dbReading, DumpStyle.Console)));
    Console.WriteLine();
}

if (pressureMillibars != null)
{
    var dbReading = new SensorReading
    {
        ReadingDateTime = readingDateTime,
        ReadingValue = pressureMillibars.Value,
        ReadingTag = "Pressure in mb"
    };

    dbContext.SensorReadings.Add(dbReading);
    await dbContext.SaveChangesAsync();
    Console.WriteLine($"Added Pressure Reading of {Green($"{dbReading.ReadingValue:0.##}mb")} to {dbFile.FullName}");
    Console.WriteLine(Cyan(ObjectDumper.Dump(dbReading, DumpStyle.Console)));
    Console.WriteLine();
}

//
// Loop thru each sensor data type, create a image chart and write to the main image
//
var recentEntryTypes = dbContext.SensorReadings.GroupBy(x => x.ReadingTag).Select(x => x.Key).OrderBy(x => x).ToList();
Console.WriteLine(
    $"{Green($"{recentEntryTypes.Count} {"Type".PluralizeIfNeeded(recentEntryTypes)}")}");
Console.WriteLine();
Console.WriteLine($"Setting Up Canvas - 400 x {650 * recentEntryTypes.Count}");
var imageInfo = new SKImageInfo(400, 650 * recentEntryTypes.Count);
var surface = SKSurface.Create(imageInfo);
var canvas = surface.Canvas;
var currentGraphVerticalOffset = 0;
Console.WriteLine();

foreach (var typeLoop in recentEntryTypes)
{
    Console.WriteLine($"Drawing Chart for {Green(typeLoop)}");

    var entries = await dbContext.SensorReadings.Where(x => x.ReadingTag == typeLoop).ToListAsync();

    var title = ChartImages.SensorTitle(entries, SKColors.Red);
    var chart = ChartImages.SensorDataChart(entries, SKColors.Red);

    canvas.DrawImage(title, 0, currentGraphVerticalOffset);
    canvas.DrawImage(chart, 0, currentGraphVerticalOffset + 50);
    canvas.Save();

    currentGraphVerticalOffset += 650;
}

//
// Save final image file
//
var filePrefixForSorting = (DateTime.MaxValue - DateTime.Now).TotalHours.ToString("00000000");
Console.WriteLine($"Photo prefix {filePrefixForSorting}");
Console.WriteLine();

var outputFileDirectory = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "Drops"));
if (!outputFileDirectory.Exists) outputFileDirectory.Create();

var outputGraphicFile = new FileInfo(Path.Combine(outputFileDirectory.FullName,
    $"{filePrefixForSorting}-{config.FileIdentifierName}-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.jpg"));
Console.WriteLine($"Saving Chart as {Green(outputGraphicFile.FullName)}");

using var image = surface.Snapshot();
using var data = image.Encode(SKEncodedImageFormat.Jpeg, 80);
await using (var stream = new FileStream(outputGraphicFile.FullName, FileMode.Create, FileAccess.Write))
{
    data.SaveTo(stream);
}

outputGraphicFile.Refresh();
Console.WriteLine($"{Green(outputGraphicFile.FullName)} - File Length {outputGraphicFile.Length}");
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

await DropboxHelpers.UploadFileToDropbox(outputGraphicFile, config.DropboxAccessToken);