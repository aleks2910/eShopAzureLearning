using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderItemsReserver.Models;
using OrderItemsReserver.Services;

namespace OrderItemsReserver
{
    public static class OrderItemsReserver
    {

        [FunctionName("OrderItemsHttpReceiverAF")]
        public static async Task<IActionResult> ProcessHttpRequest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]
        HttpRequest req,
        ILogger log,
        ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);


            var config = LoadAppSettings(context);

            //return await StoreToBlob(req, data, config["StorageConnectionString"]);


            return await StoreToCosmos(data, log);

        }

        [FunctionName("OrderItemsServiceBusReceiverAF")]
        public static async Task ProcessServiceBus(
                [ServiceBusTrigger("ordersqueue", Connection = "ServiceBusConnection")]
            string myQueueItem,
                Int32 deliveryCount,
                DateTime enqueuedTimeUtc,
                string messageId,
                ILogger log,
                ExecutionContext context)
        {
            log.LogInformation("C# Service Bus trigger function processed a request.");

            string requestBody = myQueueItem;
            dynamic data = JsonConvert.DeserializeObject(requestBody);


            var config = LoadAppSettings(context);

            await StoreToBlob(data, config["StorageConnectionString"]);
        }



        private static IConfiguration LoadAppSettings(ExecutionContext context)
        {
            return new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private static async Task StoreToCosmos(dynamic data, ILogger log)
        {
            var cosmosService = new CosmosStoreService(log);

            await cosmosService.Save(data);
        }

        private static async Task<IActionResult> StoreToBlob(dynamic data, string connsectionString = null)
        {
            var blobService = new BlobService(connsectionString == null
                ? null
                : new AzureStorageConfig() { ConnectionString = connsectionString, FileContainerName = "orders" }
            );

            try
            {
                using (var stream = new MemoryStream())
                {
                    Serialize(data, stream);
                    await blobService.Save(stream, "order_" + data.BuyerId + "_" + Environment.TickCount + ".json");
                    return new OkObjectResult("Ok");
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        static void Serialize(object value, Stream stream)
        {

            string json = JsonConvert.SerializeObject(value);

            var writer = new StreamWriter(stream);
            writer.Write(json);
            writer.Flush();
            stream.Position = 0;

        }

        private static async Task<IActionResult> StoreToBlob(HttpRequest req, dynamic data, string connsectionString = null)
        {
            req.Body.Position = 0;
            var blobService = new BlobService(connsectionString == null
                ? null
                : new AzureStorageConfig() { ConnectionString = connsectionString, FileContainerName = "orders" }
            );
            try
            {
                await blobService.Save(req.Body, "order_" + data.BuyerId + "_" + Environment.TickCount + ".json");
                return new OkObjectResult("Ok");
            }
            catch (Exception e)
            {
                return new OkObjectResult("Error - " + e.Message);
            }
        }


    }
}
