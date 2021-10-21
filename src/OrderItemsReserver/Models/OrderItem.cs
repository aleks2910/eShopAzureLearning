namespace OrderItemsReserver.Models
{
    public class OrderItem
    {
        public int CatalogItemId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Units { get; set; }
    }
}