using System;
using System.Threading;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace MultiInstanceAzureFunction
{
    public static class Function1
    {
        [FunctionName("ProcessQueueMessage")]
        public static void Run([ServiceBusTrigger("myqueue",Connection = "ServiceBusConnectionString")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"Function- started");
            Thread.Sleep(10000);
            log.LogInformation($"Function- Received message with id: {myQueueItem}");
        }
    }
}
