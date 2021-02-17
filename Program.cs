#nullable enable
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using MMALSharp;
using MMALSharp.Common;
using MMALSharp.Handlers;

namespace PiDropLapse
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var executionTime = DateTime.Now;

            Console.WriteLine($"Starting PiDropLapse {executionTime:yyyy-MM-dd-HH-mm-ss}");

            var targetDirectory = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "Drops"));

            Console.WriteLine($"Local photo directory: {targetDirectory.FullName}");

            if (!targetDirectory.Exists) targetDirectory.Create();

            var filePrefix = (DateTime.MaxValue - DateTime.Now).TotalHours.ToString("00000000");

            Console.WriteLine($"Photo prefix {filePrefix}");

            var targetFile = new FileInfo(Path.Combine(targetDirectory.FullName,
                $"{filePrefix}--Drop-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.jpg"));

            if (targetFile.Exists)
            {
                Console.WriteLine("Deleting Previous Photo");
                targetFile.Delete();
                targetFile.Refresh();
            }

            Console.WriteLine("Taking Photo");

            // Singleton initialized lazily. Reference once in your application.
            MMALCamera cam = MMALCamera.Instance;

            using (var imgCaptureHandler = new ImageStreamCaptureHandler(targetFile.FullName))
            {
                await cam.TakePicture(imgCaptureHandler, MMALEncoding.JPEG, MMALEncoding.I420);
            }

            Console.WriteLine("Cleaning Up");

            // Cleanup disposes all unmanaged resources and unloads Broadcom library. To be called when no more processing is to be done
            // on the camera.
            cam.Cleanup();

            Image bmp;

            await using (FileStream fs = new(targetFile.FullName, FileMode.Open))
            {
                bmp = Image.FromStream(fs);
                fs.Close();
            }
            
            targetFile.Delete();

            Graphics gra = Graphics.FromImage(bmp);

            string dateText = executionTime.ToString("yyyy-MM-dd HH:mm:ss");
            gra.DrawString(dateText, new Font("Verdana", 48), Brushes.Red, new PointF(20, 20));
            bmp.Save(targetFile.FullName);

            targetFile.Refresh();

            Console.WriteLine($"{targetFile.FullName} -- File Length {targetFile.Length}");

            var accessTokenFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, "PiDropLapseToken.txt"));

            if (!accessTokenFile.Exists)
            {
                Console.WriteLine($"Did not find {accessTokenFile.FullName} - skipping Dropbox processing and ending...");
                return;
            }

            Console.WriteLine($"Found {accessTokenFile.FullName} - Starting Dropbox Processing");

            var accessToken = await File.ReadAllTextAsync(accessTokenFile.FullName);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                Console.WriteLine($"{accessTokenFile.FullName} was empty - ending...");
                return;
            }

            Console.WriteLine("Starting Dropbox File List...");

            DropboxClient dropClient;

            try
            {
                dropClient = new DropboxClient(accessToken.Trim());
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
    }
}