using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpCompress.Archives.Zip;
using SharpCompress.Readers;
using SharpCompress.Archives.SevenZip;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Files.Shares;
using SharpCompress.Common;
using System.Diagnostics;

namespace MultiInstanceAzureFunction
{
    public class UnZipFileProcessor
    {
        //private readonly SecretSettings _secretSettings;
        private readonly ILogger<UnZipFileProcessor> _logger;
        private BlobContainerClient _destinationContainer;
        private string storageConnectionString;
        BlobServiceClient blobServiceClient;
        ILogger logger;

        public UnZipFileProcessor(ILogger log)
        {
            //_secretSettings = secretSettings;
            //_destinationContainer = "botoutput";
            logger = log;
            storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=vikashstg;AccountKey=/Gtd9xOKvRIb0zc/BmkSijYRoZcze5IS8qzEuFr/10r8Coub9eiVAb2Z+d1kuPTE8zpoTgL50/8E+AStKybpeA==;EndpointSuffix=core.windows.net";
            blobServiceClient = new BlobServiceClient(storageConnectionString);

            // Get and create the container for the blobs
            //BlobContainerClient _destinationContainer = blobServiceClient.GetBlobContainerClient("botoutput");
        }        

        public void ProcessInMemory()
        {
            //CloudStorageAccount storageAccunt = CloudStorageAccount.Parse(storageConnectionString);
            // Create BlobServiceClient from the connection string.


            // Get and create the container for the blobs
            BlobContainerClient outputContainerClient = blobServiceClient.GetBlobContainerClient("output");
            

            BlobContainerClient inputContainerClient = blobServiceClient.GetBlobContainerClient("botinput");


            try
            {
                List<MemoryStream> memoryStreams = new List<MemoryStream>();
                foreach (BlobItem blobitem in inputContainerClient.GetBlobs())
                {
                    //using (MemoryStream memoryStream = new MemoryStream())
                    //{
                    logger.LogInformation($"Reading 7z into memory stream with file name: {blobitem.Name}");
                    MemoryStream memoryStream = new MemoryStream();
                    BlobClient inputBlobClient = inputContainerClient.GetBlobClient(blobitem.Name);
                    inputBlobClient.DownloadTo(memoryStream);
                    memoryStream.Position = 0;
                    memoryStreams.Add(memoryStream);
                    //}
                }

                logger.LogInformation($"Reading all 7z files in memory stream done");

                //if (SevenZipArchive.IsSevenZipFile(memoryStream))
                //{

                using (SevenZipArchive archive = SevenZipArchive.Open(memoryStreams))
                {
                    foreach (SevenZipArchiveEntry entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    {
                        logger.LogInformation($"Now processing {entry.Key}");
                        BlobClient outputBlobClient = outputContainerClient.GetBlobClient(entry.Key);
                        using (var fileStream = entry.OpenEntryStream())
                        {
                            outputBlobClient.Upload(fileStream, true);
                        }
                    }
                }
                logger.LogInformation($"Uploading of all 7z files in output blob done");
                //}
            }
            catch (Exception ex)
            {
                logger.LogError($"Exceptin occured and exception is: {ex.Message}");
                throw;
            }
        }

        public void ProcessInBlob()
        {
            ShareClient shareClient= new ShareClient(storageConnectionString,"zipfileshare");
            // Get and create the container for the blobs
            BlobContainerClient outputContainerClient = blobServiceClient.GetBlobContainerClient("output");
            BlobContainerClient inputContainerClient = blobServiceClient.GetBlobContainerClient("botinput");

            try
            {
                if (shareClient.Exists())
                {
                    var root= shareClient.GetRootDirectoryClient();
                    
                    List<MemoryStream> memoryStreams = new List<MemoryStream>();
                    foreach (BlobItem blobitem in inputContainerClient.GetBlobs())
                    {
                        logger.LogInformation($"Reading 7z into memory stream with file name: {blobitem.Name}");
                        MemoryStream memoryStream = new MemoryStream();
                        BlobClient inputBlobClient = inputContainerClient.GetBlobClient(blobitem.Name);
                        inputBlobClient.DownloadTo(memoryStream);
                        memoryStream.Position = 0;
                        memoryStreams.Add(memoryStream);
                    }
                }
                else
                {
                    logger.LogInformation("Share doesn't exit");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Exceptin occured and exception is: {ex.Message}");
                throw;
            }

        }

        public void ProcessFileShare()
        {
            bool result = CleanDirectory();
            var option = new ExtractionOptions
            {
                ExtractFullPath = true,
                Overwrite = true
            };

            logger.LogInformation($"ProcessFileShare Started");
            if (result)
            {
                var watch = new Stopwatch();
                watch.Start();
                logger.LogInformation($"Checking input path...");
                //var di = new DirectoryInfo(@"/data/input");
                var di = new DirectoryInfo(@"D:\R&D\Azure\ConsoleApps\MessageSender\ZipFiles");
                if (!di.Exists)
                {
                    logger.LogInformation($"Input directory doesn't exist...");
                }
                else
                {
                    logger.LogInformation($"Input directory exist...");
                }
                
                var fi = di.GetFiles();
                
                using (SevenZipArchive archive = SevenZipArchive.Open(fi))
                {
                    var reader = archive.ExtractAllEntries();
                    reader.WriteAllToDirectory(@"/data/output", option);
                    //reader.WriteAllToDirectory(@"D:\R&D\Azure\ConsoleApps\MessageSender\vvv", option);
                    //reader.WriteAllToDirectory(@"D:\Archive\Input", option);
                    //while (reader.MoveToNextEntry())
                    //{
                    //    if (!reader.Entry.IsDirectory)
                    //    {
                    //        var entry = reader.Entry;
                    //        Console.WriteLine($"file name: {entry.Key}");
                    //    }
                    //}
                }
                watch.Stop();
                long extractionTimeInMinutes = watch.ElapsedMilliseconds / 60000;
                logger.LogInformation($"7Zip extraction completed and it took: {extractionTimeInMinutes} minutes");
            }
            else
            {
                logger.LogInformation($"ProcessFileShare Ended");
            }
        }
        public void ProcessFileShareWith7Z()
        {
            logger.LogInformation($"ProcessFileShare Started");
            var watch = new Stopwatch();
            watch.Start();
            //var filePath = $"D:\\Archive\\Input\\BlobData.7z";
            //var extractPath = $"D:\\Archive\\Output";

            var filePath = $"/exportdata/input/BlobData.7z";
            //var extractPath = $"/exportdata/input/";
            var extractPath = $"/exportdata/input/";

            //var Tasks = new List<Task>();
            //var task1 = ExtractFiles(filePath, extractPath, "*.pdf -r");
            //var task2 = ExtractFiles(filePath, extractPath, "*.txt -r");
            //var task3 = ExtractFiles(filePath, extractPath, "*.htm -r");

            //Tasks.Add(task1);
            //Tasks.Add(task2);
            //Tasks.Add(task3);

            //await Task.WhenAll(Tasks);           
            ExtractFiles(filePath, extractPath, "*.pdf -r");

            watch.Stop();
            long extractionTimeInMinutes = watch.ElapsedMilliseconds / 60000;
            logger.LogInformation($"7Zip extraction completed and it took: {extractionTimeInMinutes} minutes");
            //Console.WriteLine($"7Zip extraction completed and it took: {extractionTimeInMinutes} minutes");

        }

        private bool ExtractFiles(string filePath, string extractPath, string fileExtension)
        {
            var info = new ProcessStartInfo
            {
                FileName = $"/exportdata/input/7zz",
                //FileName = $"C:\\Program Files\\7-Zip\\7z.exe",
                //Arguments = $"x {filePath} -aos -o{extractPath} {fileExtension}",
                Arguments = $"x {filePath} -aos -o/tmp",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(info))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    var error = process.StandardError.ReadToEnd();
                    logger.LogInformation($"Error in 7zip extractin in shell script: {error}");
                    return false;// await Task.FromResult(false);
                }
            }

            return true;// await Task.FromResult(true);
        }

        public bool CleanDirectory()
        {
            var watch = new Stopwatch();
            
            logger.LogInformation($"Directory Cleaning Started");
            var outputDirectory = new DirectoryInfo(@"/data/output/BlobData");
            //var di = new DirectoryInfo(@"D:\R&D\Azure\ConsoleApps\MessageSender\vvv\BlobData");
            if (outputDirectory.Exists)
            {
                watch.Start();

                var fi = outputDirectory.GetFiles();
                foreach (FileInfo file in fi)
                {
                    if (file.Exists)
                    {
                        file.Delete();
                    }
                }

                outputDirectory.Delete();
                watch.Stop();
                long cleaningTimeInMinutes = watch.ElapsedMilliseconds / 60000;
                logger.LogInformation($"Directory Cleaning Ended and it took: {cleaningTimeInMinutes} minutes");
            }
            else
            {
                logger.LogInformation($"Directory Cleaning Ended");
            }
            return true;
        }
    }
}