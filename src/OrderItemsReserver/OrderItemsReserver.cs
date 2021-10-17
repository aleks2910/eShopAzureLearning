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
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            
            // todo: move to config
            var config = new Models.AzureStorageConfig() 
            { 
                ConnectionString = "DefaultEndpointsProtocol=https;AccountName=modeule5blobsa;AccountKey=X92YOCEYO+pI1gvqlvv7avHOPH4R0oax+8sSM80eDFeleaAy+Vfbn+TpRdmWXAV5eaToNJwUlaY2otxfcFAw6A==;EndpointSuffix=core.windows.net",
                FileContainerName = "orderscontainer"
            };

            req.Body.Position = 0;
            var blobService = new BlobService(config);
            try { 
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
