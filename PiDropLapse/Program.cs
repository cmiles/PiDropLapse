#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using MMALSharp;
using MMALSharp.Common;
using MMALSharp.Common.Utility;
using MMALSharp.Handlers;
using SharpConfig;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.RaspberryIO.Peripherals;
using Unosquare.WiringPi;

namespace PiDropLapse
{
    internal class Program
    {
        private record DhtReading(bool ValidReading, double Fahrenheit, double Celsius, double HumidityPercentage);

        private static TaskCompletionSource<object> _temperatureCompletionSource;

        private static async Task Main(string[] args)
        {
            //Simple parsing for help in the args
            if (args.Length > 0 && args.Any(x => x.ToLower().Contains("help")))
            {
                Console.WriteLine("PiDropLapse Help");
                Console.WriteLine("When you execute PiDropLapse it will attempt to:");
                Console.WriteLine(" - Take a photo and save it in a subdirectory called 'Drops'");
                Console.WriteLine(" - Upload the File to Dropbox");

                Console.WriteLine();
                Console.WriteLine("Settings are pulled from PiDropLapse.ini");
                Console.WriteLine(
                    " - If the program doesn't find the ini file when in runs a new one will be generated");
                Console.WriteLine(" - Run PiDropLapse -WriteIni to write out a new ini file");
                Console.WriteLine(" - For a Dropbox Upload to happen you need a valid AccessToken in the ini file");
                Console.WriteLine(" - Some Camera Settings can be adjusted in the ini file");
                Console.WriteLine(" - Set this up with cron to take a series of photos");

                Console.WriteLine();
                Console.WriteLine(
                    "Created by Charles Miles - see https://github.com/cmiles/PiDropLapse for more information!");

                return;
            }

            //Setup the config file
            var configFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, "PiDropLapse.ini"));

            //Simple parsing for writeini in the args
            if (args.Length > 0 && args.Any(x => x.ToLower().Contains("writeini")))
            {
                if (configFile.Exists)
                {
                    Console.WriteLine(
                        $"Sorry - Won't overwrite an existing config - delete or rename {configFile.FullName} and then use -WriteIni again to create a fresh settings file.");
                    return;
                }

                Console.WriteLine($"Writing New Settings File to {configFile.FullName}...");
                var setupConfig = new Configuration {Section.FromObject("Main", new PiDropLapseSettings())};
                setupConfig.SaveToFile(configFile.FullName);
                return;
            }


            var executionTime = DateTime.Now;
            Console.WriteLine($"Starting PiDropLapse Starting - {executionTime:yyyy-MM-dd-HH-mm-ss}");


            Console.WriteLine($"Getting Config File - {configFile.FullName}");
            if (!configFile.Exists)
            {
                Console.WriteLine("No Config File Found - writing Defaults");
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
                Console.WriteLine($"Trouble Parsing Config - Using Defaults... Error {e}");
                config = new PiDropLapseSettings();
            }

            Console.WriteLine("Config:");
            Console.WriteLine(ObjectDumper.Dump(config, new DumpOptions {ExcludeProperties = new List<string> { "DropboxAccessToken" }, DumpStyle = DumpStyle.Console}));


            var targetDirectory = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "Drops"));
            Console.WriteLine($"Local photo directory: {targetDirectory.FullName}");
            if (!targetDirectory.Exists) targetDirectory.Create();


            var filePrefix = (DateTime.MaxValue - DateTime.Now).TotalHours.ToString("00000000");
            Console.WriteLine($"Photo prefix {filePrefix}");


            var targetFile = new FileInfo(Path.Combine(targetDirectory.FullName,
                $"{filePrefix}--{config.FileIdentifierName}--{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.jpg"));
            if (targetFile.Exists)
            {
                Console.WriteLine("Deleting Previous Photo");
                targetFile.Delete();
                targetFile.Refresh();
            }


            Console.WriteLine("Initializing Camera");
            MMALCamera piCamera = MMALCamera.Instance;
            MMALCameraConfig.ISO = config.Iso;
            MMALCameraConfig.ShutterSpeed = config.ShutterSpeed;
            MMALCameraConfig.Rotation = config.Rotation;
            MMALCameraConfig.ExposureCompensation = config.ExposureCompensation;
            if (MMALCameraConfig.Rotation == 0 || MMALCameraConfig.Rotation == 180)
                MMALCameraConfig.StillResolution = new Resolution(config.LongEdgeResolution,
                    config.LongEdgeResolution / 16 * 9);
            if (MMALCameraConfig.Rotation == 90 || MMALCameraConfig.Rotation == 270)
                MMALCameraConfig.StillResolution =
                    new Resolution(config.LongEdgeResolution / 16 * 9, config.LongEdgeResolution);

            piCamera.ConfigureCameraSettings();

            using (var imgCaptureHandler = new ImageStreamCaptureHandler(targetFile.FullName))
            {
                piCamera.ConfigureCameraSettings(imgCaptureHandler);
                Console.WriteLine("Taking Photo");
                await piCamera.TakePicture(imgCaptureHandler, MMALEncoding.JPEG, MMALEncoding.I420);
            }

            Console.WriteLine("Cleaning Up");
            piCamera.Cleanup();

            //Console.WriteLine("Bootstrapping");

            //Pi.Init<BootstrapWiringPi>();

            //Console.WriteLine("Creating Sensor");

            //var dht22Sensor = DhtSensor.Create(DhtType.Dht22, Pi.Gpio[BcmPin.Gpio04]);

            //Console.WriteLine("New Taskcompletion");

            //var temperatureCompletionSource = new TaskCompletionSource<DhtReading>();

            //dht22Sensor.OnDataAvailable += (_, e) =>
            //{
            //    Console.WriteLine("Temperature reading event triggered");

            //    if (!e.IsValid)
            //    {
            //        return;
            //    }

            //    Console.WriteLine($"DHT22 Temperature: {e.Temperature:0.00}°C {e.TemperatureFahrenheit:0.00}°F Humidity: {e.HumidityPercentage:P0}");

            //    temperatureCompletionSource.TrySetResult(new DhtReading(true, e.TemperatureFahrenheit, e.Temperature, e.HumidityPercentage));
            //};

            //Console.WriteLine("Sensor Start");

            //dht22Sensor.Start();

            //Console.WriteLine("TCS Wait");

            //var temperatureReading = await temperatureCompletionSource.Task;

            Console.WriteLine("Loading file to write date");
            Image bmp;

            //Read the photo, delete it - draw the text on it and save it
            await using (FileStream fs = new(targetFile.FullName, FileMode.Open))
            {
                bmp = Image.FromStream(fs);
                fs.Close();
            }

            targetFile.Delete();

            //Write the date into the top left of the photo using approximately 1/3
            //of the width of the photo or a minimum of Verdana 12

            Console.WriteLine("Writing date");
            Graphics g = Graphics.FromImage(bmp);
            var width = (int) g.VisibleClipBounds.Width;
            Console.WriteLine($"Photo Width {width}");

            string dateText = executionTime.ToString("yyyy-MM-dd HH:mm:ss");
            var adjustedFont = TryAdjustFontSizeToFitWidth(g, dateText, new Font("Verdana", 12), width / 3, 12, 128);
            Console.WriteLine($"Adjusted Font Size - {adjustedFont.Size}");
            g.DrawString(dateText, adjustedFont, Brushes.Red, new PointF(20, 20));

            Console.WriteLine("Saving file with date written");
            bmp.Save(targetFile.FullName);
            targetFile.Refresh();
            Console.WriteLine($"{targetFile.FullName} -- File Length {targetFile.Length}");


            //If we have a DropboxAccessToken upload to Dropbox
            if (string.IsNullOrWhiteSpace(config.DropboxAccessToken))
            {
                Console.WriteLine("Did not find a DropboxAccessToken - skipping Dropbox processing and ending...");
                return;
            }

            Console.WriteLine("Found a Dropbox Access Token - Starting Dropbox Connection");

            DropboxClient dropClient;
            try
            {
                dropClient = new DropboxClient(config.DropboxAccessToken.Trim());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

            Console.WriteLine("Starting Dropbox Upload...");
            var dropboxUploadResult =
                await dropClient.Files.UploadAsync(new CommitInfo($@"/{targetFile.Name}"), targetFile.OpenRead());
            Console.WriteLine($"Dropbox File Uploaded {dropboxUploadResult.PathDisplay}");
        }

        /// <summary>
        ///     Returns a font (inside the specified range) that is the largest font
        ///     that will fit inside the given width.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="stringToDraw"></param>
        /// <param name="referenceFont"></param>
        /// <param name="maxWidth"></param>
        /// <param name="minimumFontSize"></param>
        /// <param name="maximumFontSize"></param>
        /// <returns></returns>
        public static Font TryAdjustFontSizeToFitWidth(Graphics g, string stringToDraw, Font referenceFont,
            int maxWidth, int minimumFontSize, int maximumFontSize)
        {
            //Based on https://docs.microsoft.com/en-us/previous-versions/bb986765(v=msdn.10)?redirectedfrom=MSDN 
            //with thanks to https://stackoverflow.com/questions/15571715/auto-resize-font-to-fit-rectangle/30567857 for the link;

            Font testFont = new(referenceFont.Name, referenceFont.Size, referenceFont.Style);

            for (var testSize = maximumFontSize; testSize >= minimumFontSize; testSize--)
            {
                testFont = new Font(referenceFont.Name, testSize, referenceFont.Style);

                // Test the string with the new size
                var adjustedSizeNew = g.MeasureString(stringToDraw, testFont);

                if (maxWidth > Convert.ToInt32(adjustedSizeNew.Width))
                    // First font to fit - return it
                    return testFont;
            }

            return testFont;
        }
        
    }
}