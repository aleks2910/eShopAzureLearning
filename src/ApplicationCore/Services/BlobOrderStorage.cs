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

namespace Microsoft.eShopWeb.ApplicationCore.Services
{
    public class BlobOrderStorage : IBlobOrderStorage
    {

        private ILogger<BlobOrderStorage> _log { get; }

        public BlobOrderStorage(ILogger<BlobOrderStorage> log)
        {
            _log = log;
        }        

        public async Task<bool> SaveOrderToBlob(Order order)
        {
            _log.LogInformation("Calll App Function...");

            //todo: move to config
            string apiUrl = "https://orderitemsreserveraf.azurewebsites.net/api/OrderItemsReserverAF";            

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
