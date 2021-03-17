using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PiDropPhoto;
using PiDropSimpleSensorReport;
using PiDropUtility;
using SkiaSharp;
using static Crayon.Output;


var dbFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, "PiDropLapse.db"));
var dbContext = new SensorReadingContext(dbFile.FullName);
dbContext.Database.EnsureCreated();
if (!dbFile.Exists)
{
    Console.WriteLine(Red($"Database File {dbFile.FullName} Not Found?"));
    return;
}

Console.WriteLine($"Using Database File {Green(dbFile.FullName)}");
Console.WriteLine();
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

var photoFilePrefix = (DateTime.MaxValue - DateTime.Now).TotalHours.ToString("00000000");
Console.WriteLine($"Photo prefix {photoFilePrefix}");
Console.WriteLine();
var outputFileDirectory = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "Drops"));
if (!outputFileDirectory.Exists) outputFileDirectory.Create();
var outputGraphicFile = Path.Combine(outputFileDirectory.FullName,
    $"{photoFilePrefix}-SimpleSensorChart-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.jpg");
Console.WriteLine($"Saving Chart as {Green(outputGraphicFile)}");
using var image = surface.Snapshot();
using var data = image.Encode(SKEncodedImageFormat.Jpeg, 80);
await using var stream =
    File.OpenWrite(outputGraphicFile);
data.SaveTo(stream);