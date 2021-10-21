
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Newtonsoft.Json;
using OrderItemsReserver.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;


namespace Microsoft.eShopWeb.IntegrationTests
{

    public class GenerateBrandsCacheKey
    {
        [Fact]
        public async Task ReturnsBrandsCacheKeyAsync()
        {
            IBlobOrderStorage storage = new BlobOrderStorage(null);

            Order order = GetOrder();

            Assert.True(await storage.SaveOrderToBlob(order));
        }

        private static Order GetOrder()
        {
            return new Order("tetsBuyerId", new Address("123 Main St.", "Kent", "OH", "United States", "44240"),
                new List<OrderItem>() {
                        new OrderItem(new CatalogItemOrdered(1, "some product1", "prdID1"), 10, 1),
                        new OrderItem(new CatalogItemOrdered(2, "some product2", "prdID1"), 11, 2),
            });
        }

        [Fact]
        public async Task SaveBlobTest()
        {
            // todo: move to config
            var config = new OrderItemsReserver.Models.AzureStorageConfig()
            {
                ConnectionString = "DefaultEndpointsProtocol=https;AccountName=modeule5blobsa;AccountKey=X92YOCEYO+pI1gvqlvv7avHOPH4R0oax+8sSM80eDFeleaAy+Vfbn+TpRdmWXAV5eaToNJwUlaY2otxfcFAw6A==;EndpointSuffix=core.windows.net",
                FileContainerName = "orderscontainer"
            };
            
            var blobService = new BlobService(config);

            var order = GetOrder();           

            var stream = new MemoryStream();            
            Serialize(order, stream);

            stream.Position = 0;
            try
            {
                await blobService.Save(stream, "testOrder2");
            }catch (Exception ex ) 
            {
                throw ex;
            }            
        }

        [Fact]
        public async Task CosmosStoreServiceTest() 
        {
            var service = new CosmosStoreService();
            dynamic dynamicOrder = GetOrder();
            await service.Save(dynamicOrder);
        }      

        static void Serialize(object value, Stream stream)
        {

            string json = JsonConvert.SerializeObject(value);
            
            var writer = new StreamWriter(stream);
            writer.Write(json);
            writer.Flush();
            stream.Position = 0;            
            
        }

    }

}
