using System;
using System.Xml.Serialization;

namespace StefanOlsen.Commerce.CatalogFeed.GoogleMerchant.Models
{
    [Serializable]
    [XmlType("shipping", Namespace = Constants.NamespaceGoogleMerchant)]
    public class Shipping
    {
        [XmlElement("country", Namespace = Constants.NamespaceGoogleMerchant)]
        public string Country { get; set; }

        [XmlElement("service", Namespace = Constants.NamespaceGoogleMerchant)]
        public string Service { get; set; }

        [XmlElement("price", Namespace = Constants.NamespaceGoogleMerchant)]
        public string Price { get; set; }
    }
}
