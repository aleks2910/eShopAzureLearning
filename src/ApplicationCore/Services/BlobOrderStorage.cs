using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Microsoft.eShopWeb.ApplicationCore.Services
{
    public class BlobOrderStorage : IBlobOrderStorage
    {
        // connection string to your Service Bus namespace
        static string connectionString = "Endpoint=sb://eshopnamespacealeks.servicebus.windows.net/;SharedAccessKeyName=ListenerConnect;SharedAccessKey=XCMZ8EvkcgU+bbr5MzvbYdHwxPChzHk7hKhmWkuAj7g=";

        // name of your Service Bus queue
        static string queueName = "ordersqueue";

        // the client that owns the connection and can be used to create senders and receivers
        static ServiceBusClient client;

        // the sender used to publish messages to the queue
        static ServiceBusSender sender;



        private ILogger<BlobOrderStorage> _log { get; }

        public BlobOrderStorage(ILogger<BlobOrderStorage> log)
        {
            _log = log;
        }

        public async Task<bool> SendToQueue(Order order)
        {
            _log.LogInformation("Send to queue...");

            
            try
            {

                client = new ServiceBusClient(connectionString);
                sender = client.CreateSender(queueName);

                string messagePayload = JsonConvert.SerializeObject(order);
                ServiceBusMessage message = new ServiceBusMessage(messagePayload);
                try
                {   
                    await sender.SendMessageAsync(message).ConfigureAwait(false);
                    // Use the producer client to send the batch of messages to the Service Bus queue
                    await sender.SendMessageAsync(message);
                    
                }
                finally
                {
                    // Calling DisposeAsync on client types is required to ensure that network
                    // resources and other unmanaged objects are properly cleaned up.
                    await sender.DisposeAsync();
                    await client.DisposeAsync();
                }

                _log.LogInformation("send to queue -  Succeded!");
                return true;
            }
            catch (WebException ex)
            {
                // log errorText
                _log.LogError("Error sending to queue! " + ex.Message);
            }
            return false;
        }

        public async Task<bool> CallAppFunc(Order order)
        {
            _log.LogInformation("Calll App Function...");

            //todo: move to config
            string apiUrl = "https://orderitemsreserveraleks.azurewebsites.net/api/OrderItemsReserverAF";            

            var data = JsonConvert.SerializeObject(order);
            HttpClient client = new HttpClient();
            var postData = JsonConvert.SerializeObject(data);
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            try
            {
                var response = await client.PostAsync(apiUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    _log.LogError("Function call failed - " + response.StatusCode);
                    return false;
                }                   
                _log.LogInformation("Function call Succeded!");
                return true;
            }
            catch (WebException ex)
            {
                // log errorText
                _log.LogError("Error call azure function! " + ex.Message);
                
                return false;
            }
        }
    }
}
