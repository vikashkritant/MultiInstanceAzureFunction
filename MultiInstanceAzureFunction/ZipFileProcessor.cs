using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiInstanceAzureFunction
{
    public class ZipFileProcessor
    {
        //private readonly SecretSettings _secretSettings;
        private readonly ILogger<ZipFileProcessor> _logger;
        private BlobContainerClient _destinationContainer;
        private string storageConnectionString;

        public ZipFileProcessor() 
        {
            //_secretSettings = secretSettings;
            //_destinationContainer = "botoutput";
            //_logger = logger;
            storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=vikashstg;AccountKey=sUMdVJISRTdypx4Aq81EGRJ0362X2aeHql7RmeXnofzzfuVpb4DXWrybT02nXyAdDuWQYNy9ykYK+AStbvatug==;EndpointSuffix=core.windows.net";
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);

            // Get and create the container for the blobs
            BlobContainerClient _destinationContainer = blobServiceClient.GetBlobContainerClient("botoutput");
        }

        public async Task ProcessFile(Stream blobStream)
        {
            if (ZipArchive.IsZipFile(blobStream))
            {
                var zipReaderOptions = new ReaderOptions()
                {
                    ArchiveEncoding = new ArchiveEncoding(Encoding.UTF8, Encoding.UTF8),
                    LookForHeader = true
                };

                _logger.LogInformation("Blob is a zip file; beginning extraction....");
                blobStream.Position = 0;

                using var reader = ZipArchive.Open(blobStream, zipReaderOptions);

                await ExtractArchiveFiles(reader.Entries);
            }
        }

        protected async Task ExtractArchiveFiles(IEnumerable<IArchiveEntry> archiveEntries)
        {
            foreach (var archiveEntry in archiveEntries.Where(entry => !entry.IsDirectory))
            {
                _logger.LogInformation($"Now processing {archiveEntry.Key}");

                NameValidator.ValidateBlobName(archiveEntry.Key);

                var blockBlob = _destinationContainer.GetBlobClient(archiveEntry.Key);
                await using var fileStream = archiveEntry.OpenEntryStream();
                await blockBlob.UploadAsync(fileStream);

                _logger.LogInformation(
                    $"{archiveEntry.Key} processed successfully and moved to destination container");
            }
        }

        public async void Process()
        {
            // Create BlobServiceClient from the connection string.
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);

            // Get and create the container for the blobs
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient("botinput");
            
            IAsyncEnumerable<BlobItem> segment = container.GetBlobsAsync(prefix: "");
            await foreach (BlobItem blobItem in segment)
            {
                Console.WriteLine(blobItem.Name);
                BlobClient blob = container.GetBlobClient(blobItem.Name);
                //var stream = await blob.OpenReadAsync();
                using(MemoryStream blobStream =new MemoryStream())
                {                    
                    await blob.DownloadToAsync(blobStream);
                    await ProcessFile(blobStream);
                }
                
                
            }

        }
    }
}