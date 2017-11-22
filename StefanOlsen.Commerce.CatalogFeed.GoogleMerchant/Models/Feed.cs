using System;
using System.Xml.Serialization;

namespace StefanOlsen.Commerce.CatalogFeed.GoogleMerchant.Models
{
    [Serializable]
    [XmlRoot("feed", Namespace = Constants.NamespaceAtom)]
    public class Feed
    {
        [XmlElement("title", Namespace = Constants.NamespaceAtom)]
        public string Title { get; set; }

        [XmlElement("link", Namespace = Constants.NamespaceAtom)]
        public string Link { get; set; }

        [XmlElement("updated", Namespace = Constants.NamespaceAtom)]
        public DateTime Updated { get; set; }

        [XmlElement("entry")]
        public Enumerable<Entry> Entries { get; set; }
    }
}
