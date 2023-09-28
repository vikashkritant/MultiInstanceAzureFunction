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
            storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=vikashstg;AccountKey=sUMdVJISRTdypx4Aq81EGRJ0362X2aeHql7RmeXnofzzfuVpb4DXWrybT02nXyAdDuWQYNy9ykYK+AStbvatug==;EndpointSuffix=core.windows.net";
            blobServiceClient = new BlobServiceClient(storageConnectionString);

            // Get and create the container for the blobs
            //BlobContainerClient _destinationContainer = blobServiceClient.GetBlobContainerClient("botoutput");
        }

        //public async Task ProcessFile(Stream blobStream)
        //{
        //    if (ZipArchive.IsZipFile(blobStream))
        //    {
        //        var zipReaderOptions = new ReaderOptions()
        //        {
        //            ArchiveEncoding = new ArchiveEncoding(Encoding.UTF8, Encoding.UTF8),
        //            LookForHeader = true
        //        };

        //        _logger.LogInformation("Blob is a zip file; beginning extraction....");
        //        blobStream.Position = 0;

        //        using var reader = ZipArchive.Open(blobStream, zipReaderOptions);

        //        await ExtractArchiveFiles(reader.Entries);
        //    }
        //}

        //protected async Task ExtractArchiveFiles(IEnumerable<IArchiveEntry> archiveEntries)
        //{
        //    foreach (var archiveEntry in archiveEntries.Where(entry => !entry.IsDirectory))
        //    {
        //        _logger.LogInformation($"Now processing {archiveEntry.Key}");

        //        NameValidator.ValidateBlobName(archiveEntry.Key);

        //        var blockBlob = _destinationContainer.GetBlobClient(archiveEntry.Key);
        //        await using var fileStream = archiveEntry.OpenEntryStream();
        //        await blockBlob.UploadAsync(fileStream);

        //        _logger.LogInformation(
        //            $"{archiveEntry.Key} processed successfully and moved to destination container");
        //    }
        //}

        public async void Process()
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
                        Console.WriteLine($"Now processing {entry.Key}");
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
            }

        }
    }
}