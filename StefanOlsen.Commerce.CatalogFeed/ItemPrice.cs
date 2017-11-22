using EPiServer.Core;

namespace StefanOlsen.Commerce.CatalogFeed
{
    public class ItemPrice
    {
        public string Code { get; set; }
        public ContentReference ContentLink { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SalePrice { get; set; }
        public string Currency { get; set; }
    }
}
