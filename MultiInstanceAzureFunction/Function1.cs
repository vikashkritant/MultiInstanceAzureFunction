using System;
using System.IO;
using System.Threading;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace MultiInstanceAzureFunction
{
    public class Function1
    {
        private string messageId=null;

        [FunctionName("ProcessQueueMessage")]
        public void Run([ServiceBusTrigger("zipqueue",Connection = "ServiceBusConnectionString")]
            ServiceBusReceivedMessage msg,
            //string msg, 
            //string messageId,
            ILogger log)
        {            
            log.LogInformation($"Function- Received message with id: {msg.MessageId} and variable message id is {messageId}");
            if (messageId == msg.MessageId)
            {
                log.LogInformation($"Variable message id is: {messageId}");
                log.LogInformation($"Skipping message as received message id is: {msg.MessageId} and delivery count is: {msg.DeliveryCount}");
            }
            if (string.IsNullOrEmpty(messageId))
            {
                messageId = msg.MessageId;
                log.LogInformation($"Set the message id as: {messageId}");
                try
                {
                    //echo "Mounting drive"
                    //az webapp config storage-account add--resource - group vikashrg--name MultiInstanceAzureFunction --custom - id myfileshareid--storage - type AzureFiles--share - name myfileshare--account - name vikashrg8582--mount - path / vikash - files--access - key jzyAF9ufbN4TyM6nVbXXI4XoXZiFJklKXl82VFeHI2riAgBuVQCVkg2I / zIh7bQxpL44LUY2uvFb + AStsom9pA ==
                    //echo "Mounting drive done"
                    UnZipFileProcessor unzipper = new UnZipFileProcessor(log);
                    unzipper.CleanDirectory();
                }
                catch (Exception ex)
                {
                    log.LogError($"Error is:  {ex.Message}");
                }
            }
        }
    }
}
