using AutoMapper;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderItemsReserver.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OrderItemsReserver.Services
{
    public class CosmosStoreService
    {

        private static readonly string _endpointUri = "https://eshopalekscosmos.documents.azure.com:443/";
        private static readonly string _primaryKey = "3gEWXHGqXzt1iTbSYOtOxRfi0yf2eqmnUEe16xkHvsQTIkoobc8vlmfyFc4MSETku5DUbr2YqJQpaaNRV18IwQ==";
        private string databaseId = "eShopDB";
        private string containerId = "Orders";
        private string partitionKey = "BuyerId";
        private ILogger log;

        public CosmosStoreService(ILogger log)
        {
            this.log = log;
        }

        public async Task Save(dynamic orderData)
        {
            log.LogInformation("Stage 3");
            OrderEntry orderEntry = CreateOrderEntry(orderData);
            log.LogInformation("Stage 4");

            try
            {
                await SaveOrder(orderEntry);
            }
            catch (Exception ee )
             {
                log.LogError("Saving order error! " + ee.Message, ee);
            }
            
        }

        private async Task SaveOrder(OrderEntry orderEntry)
        {
            using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
            {                
                DatabaseResponse databaseResponse = await client.CreateDatabaseIfNotExistsAsync(databaseId);                
                Database targetDatabase = databaseResponse.Database;                
                IndexingPolicy indexingPolicy = new IndexingPolicy
                {
                    IndexingMode = IndexingMode.Consistent,
                    Automatic = true,
                    IncludedPaths =
                    {
                        new IncludedPath
                        {
                            Path = "/*"
                        }
                    }
                };                
                var containerProperties = new ContainerProperties(containerId, "/" + partitionKey)
                {
                    IndexingPolicy = indexingPolicy
                };

                var containerResponse = await targetDatabase.CreateContainerIfNotExistsAsync(containerProperties, 10000);
                var container = containerResponse.Container;                                

                try
                {                    
                    ItemResponse<OrderEntry> wakefieldFamilyResponse = await container.ReadItemAsync<OrderEntry>(orderEntry.Id, new PartitionKey(orderEntry.Id));
                    Console.WriteLine("Item in database with id: {0} already exists\n", wakefieldFamilyResponse.Resource.Id);
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                { 
                    ItemResponse<OrderEntry> wakefieldFamilyResponse = await container.CreateItemAsync<OrderEntry>(orderEntry, new PartitionKey(orderEntry.BuyerId));
                }
            }
        }

        private OrderEntry CreateOrderEntry(dynamic orderData)
        {

            var json = JsonConvert.SerializeObject(orderData);
            OrderEntry  result = JsonConvert.DeserializeObject<OrderEntry>(json);

            var i = 0;
            foreach (var orderitem in result.OrderItems)
            {
                result.FinalPrice += orderitem.UnitPrice * orderitem.Units;
                orderitem.CatalogItemId = orderData.OrderItems[i++].ItemOrdered.CatalogItemId;
            }



            return result;
        }

        private async Task<Container> InitializeDb()
        {
            using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
            {
                DatabaseResponse databaseResponse = await client.CreateDatabaseIfNotExistsAsync(databaseId);
                Database targetDatabase = databaseResponse.Database;
                // await Console.Out.WriteLineAsync($"Database Id:\t{targetDatabase.Id}");
                IndexingPolicy indexingPolicy = new IndexingPolicy
                {
                    IndexingMode = IndexingMode.Consistent,
                    Automatic = true,
                    IncludedPaths =
                    {
                        new IncludedPath
                        {
                            Path = "/*"
                        }
                    }
                };

                var containerProperties = new ContainerProperties(containerId, "/" + databaseId)
                {
                    IndexingPolicy = indexingPolicy
                };

                var containerResponse = await targetDatabase.CreateContainerIfNotExistsAsync(containerProperties, 10000);
                var customContainer = containerResponse.Container;
                return customContainer;
                //await Console.Out.WriteLineAsync($"Custom Container Id:\t{customContainer.Id}");
            }
        }
    }
}
