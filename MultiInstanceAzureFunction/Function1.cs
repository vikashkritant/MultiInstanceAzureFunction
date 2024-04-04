using System;
using System.IO;
using System.Threading;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;

namespace MultiInstanceAzureFunction
{
    public class Function1
    {

        [FunctionName("ProcessQueueMessage")]
        public void Run([ServiceBusTrigger("zipqueue",Connection = "ServiceBusConnectionString")]
            ServiceBusReceivedMessage msg,
            ServiceBusMessageActions messageActions,
            //string msg, 
            //string messageId,
            ILogger log)
        {
            messageActions.CompleteMessageAsync(msg);
            log.LogInformation($"MultiInstanceFunction- Received message with id: {msg.MessageId}");           
            
            try
            {
                //echo "Mounting drive"
                //az webapp config storage-account add--resource - group vikashrg--name MultiInstanceAzureFunction --custom - id myfileshareid--storage - type AzureFiles--share - name myfileshare--account - name vikashrg8582--mount - path / vikash - files--access - key jzyAF9ufbN4TyM6nVbXXI4XoXZiFJklKXl82VFeHI2riAgBuVQCVkg2I / zIh7bQxpL44LUY2uvFb + AStsom9pA ==
                //echo "Mounting drive done"
                UnZipFileProcessor unzipper = new UnZipFileProcessor(log);
                unzipper.ProcessFileShare();
            }
            catch (Exception ex)
            {
                log.LogError($"Error is:  {ex.Message} and stacktrace is {ex.StackTrace}");
            }

        }
    }
}
