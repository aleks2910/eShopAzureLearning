using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderItemsReserver.Models
{
    public class OrderEntry
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "BuyerId")]
        public string BuyerId { get; set; }

        public Address ShipToAddress { get; set; }

        public OrderItem[] OrderItems { get; set; }

        public decimal FinalPrice { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
