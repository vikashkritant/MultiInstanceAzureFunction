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

        public void Process()
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

        public void ProcessFileShare()
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
    }
}