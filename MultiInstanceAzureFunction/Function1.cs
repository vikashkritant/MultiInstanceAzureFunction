using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace MultiInstanceAzureFunction
{
    public static class Function1
    {
        [FunctionName("ProcessQueueMessage")]
        public static void Run([ServiceBusTrigger("myqueue",Connection = "ServiceBusConnectionString")]string myQueueItem, ILogger log)
        {
            string instanceId = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID");
            log.LogInformation($"Function-{instanceId} Received message with id: {myQueueItem}");
        }
    }
}
