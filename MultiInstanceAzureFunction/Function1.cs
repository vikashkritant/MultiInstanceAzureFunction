using System;
using System.IO;
using System.Threading;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace MultiInstanceAzureFunction
{
    public static class Function1
    {
        [FunctionName("ProcessQueueMessage")]
        public static void Run([ServiceBusTrigger("zipqueue",Connection = "ServiceBusConnectionString")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"Function- started");
            
            //UnZipFileProcessor unzipper = new UnZipFileProcessor( log);
            //unzipper.Process();
            
            log.LogInformation($"Function- Received message with id: {myQueueItem}");
            try
            {
                //string[] filePaths = Directory.GetFiles(@"/fx-files");
                //string fileName = filePaths[0];

                //log.LogInformation($"Files in file share are: {fileName}");
                UnZipFileProcessor unzipper = new UnZipFileProcessor( log);
                unzipper.ProcessFileShare();
            }
            catch (Exception ex)
            {
                log.LogError($"Error is:  {ex.Message}");
            }
        }
    }
}
