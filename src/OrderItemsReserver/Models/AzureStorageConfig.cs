using System;
using System.Collections.Generic;
using System.Text;

namespace OrderItemsReserver.Models
{
    public class AzureStorageConfig
    {
        public string ConnectionString { get; set; }
        public string FileContainerName { get; set; }
    }

}
