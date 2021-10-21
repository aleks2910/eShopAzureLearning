using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderItemsReserver.Services;

namespace OrderItemsReserver
{
    public static class OrderItemsReserver
    {
        [FunctionName("OrderItemsReserverAF")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation("Stage 1");
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            //return await StoreToBlob(req, data);
            log.LogInformation("Stage 2");
            return await StoreToCosmos(data, log);

        }

        private static async Task StoreToCosmos(dynamic data, ILogger log)
        {
            var cosmosService = new CosmosStoreService(log);

            await cosmosService.Save(data);


        }

        private static async Task<IActionResult> StoreToBlob(HttpRequest req, dynamic data)
        {
            req.Body.Position = 0;
            var blobService = new BlobService(null);
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
