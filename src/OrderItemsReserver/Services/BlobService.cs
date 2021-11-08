using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using OrderItemsReserver.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OrderItemsReserver.Services
{
    public class BlobService
    {

        private readonly AzureStorageConfig storageConfig;
        private readonly AzureStorageConfig DefaultConfig = new AzureStorageConfig()
        {
            ConnectionString = "<hiddewn>",
            FileContainerName = "orders"
        };

        public BlobService(AzureStorageConfig storageConfig)
        {
            this.storageConfig = storageConfig ?? DefaultConfig;
        }


        public Task Initialize()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConfig.ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(storageConfig.FileContainerName);
            return containerClient.CreateIfNotExistsAsync();
        }

        public Task Save(Stream fileStream, string name)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConfig.ConnectionString);

            // Get the container (folder) the file will be saved in
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(storageConfig.FileContainerName);

            // Get the Blob Client used to interact with (including create) the blob
            BlobClient blobClient = containerClient.GetBlobClient(name);

            // Upload the blob
            return blobClient.UploadAsync(fileStream);
        }
    }
}
