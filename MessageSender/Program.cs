// See https://aka.ms/new-console-template for more information
using Azure.Messaging.ServiceBus;

Console.WriteLine("Hello, World!");
MessageSender sender = new MessageSender();
await sender.SendMessagesAsync();

public class MessageSender
{
    public async Task SendMessagesAsync()
    {
        try
        {            
            var serviceBusConnectionString = "Endpoint=sb://vikashtestsb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=OhEaPssx88ZC0CMsHkApTs87BUh90ASjZFu1b6fGFnI=";
            var serviceBusClient = new ServiceBusClient(serviceBusConnectionString);

            // create the sender
            ServiceBusSender sender = serviceBusClient.CreateSender("myqueue");

            for (int msgNumber = 0; msgNumber < 10; msgNumber++)
            {           
                // Create a new message to send to the queue
                string messageBody = $"Message {msgNumber}";
                var message = new ServiceBusMessage(messageBody);

                // Write the body of the message to the console
                //this._logger.LogInformation($"Sending message: {messageBody}");

                // Send the message to the queue
                await sender.SendMessageAsync(message);
                Console.WriteLine($"Pushed message with id: {msgNumber}");                              
                
            }
        }
        catch (Exception exception)
        {
            
        }
    }
}