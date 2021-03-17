using System;
using System.IO;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using static Crayon.Output;

namespace PiDropUtility
{
    public static class DropboxHelpers
    {
        public static async Task UploadFileToDropbox(FileInfo photoTargetFile, string dropboxAccessToken)
        {
            Console.WriteLine($"Found a Dropbox Access Token - {Green("Opening Dropbox Connection")}");
            DropboxClient dropClient;
            try
            {
                dropClient = new DropboxClient(dropboxAccessToken.Trim());
            }
            catch (Exception e)
            {
                Console.WriteLine(Red($"Dropbox Exception:{Environment.NewLine}{e}"));
                return;
            }

            Console.WriteLine("Starting Dropbox Upload...");
            var dropboxUploadResult =
                await dropClient.Files.UploadAsync(new CommitInfo($@"/{photoTargetFile.Name}"),
                    photoTargetFile.OpenRead());
            Console.WriteLine(
                $"Dropbox File Uploaded (PiDropLapse App Directory){Green(dropboxUploadResult.PathDisplay)}");
            Console.WriteLine();
        }
    }
}