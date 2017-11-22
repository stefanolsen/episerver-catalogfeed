using System;
using EPiServer.Data;
using EPiServer.Data.Dynamic;

namespace StefanOlsen.Commerce.CatalogFeed.Data
{
    [EPiServerDataStore(AutomaticallyCreateStore = true, AutomaticallyRemapStore = true)]
    public class CatalogFeedItem : IDynamicData
    {
        public Identity Id { get; set; }

        public Uri BlobId { get; set; }

        [EPiServerDataIndex]
        public string ProviderName { get; set; }

        [EPiServerDataIndex]
        public string MarketId { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ExpireDate { get; set; }
    }
}
