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
                //echo "Mounting drive"
                //az webapp config storage-account add--resource - group vikashrg--name MultiInstanceAzureFunction --custom - id myfileshareid--storage - type AzureFiles--share - name myfileshare--account - name vikashrg8582--mount - path / vikash - files--access - key jzyAF9ufbN4TyM6nVbXXI4XoXZiFJklKXl82VFeHI2riAgBuVQCVkg2I / zIh7bQxpL44LUY2uvFb + AStsom9pA ==
                //echo "Mounting drive done"
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
