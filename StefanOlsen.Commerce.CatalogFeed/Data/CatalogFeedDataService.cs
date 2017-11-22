using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Data;
using EPiServer.Data.Dynamic;

namespace StefanOlsen.Commerce.CatalogFeed.Data
{
    public class CatalogFeedDataService : ICatalogFeedDataService
    {
        private readonly DynamicDataStoreFactory _dynamicDataStoreFactory;
        private const string StoreName = "FeedItemStore";

        public CatalogFeedDataService(DynamicDataStoreFactory dynamicDataStoreFactory)
        {
            _dynamicDataStoreFactory = dynamicDataStoreFactory;
        }

        public CatalogFeedItem Create(Uri blobId, string providerName, string marketId)
        {
            var catalogFeedItem = new CatalogFeedItem
            {
                Id = Identity.NewIdentity(),
                BlobId = blobId,
                ProviderName = providerName,
                MarketId = marketId,
                CreatedDate = DateTime.UtcNow,
                ExpireDate = DateTime.UtcNow.AddDays(1)
            };

            var store = GetStore();
            store.Save(catalogFeedItem);

            return catalogFeedItem;
        }

        public CatalogFeedItem Get(string feedItemId)
        {
            if (!Identity.TryParse(feedItemId, out Identity identity))
            {
                return null;
            }

            var store = GetStore();
            var feedItem = store.Load<CatalogFeedItem>(identity);

            return feedItem;
        }

        public CatalogFeedItem Get(string providerName, string marketId)
        {
            var store = GetStore();

            var parameters = new Dictionary<string, object>
            {
                {"ProviderName", providerName},
                {"MarketId", marketId}
            };

            var feedItem = store.Find<CatalogFeedItem>(parameters)
                .OrderByDescending(item => item.CreatedDate)
                .FirstOrDefault();

            return feedItem;
        }

        public IEnumerable<CatalogFeedItem> GetAll()
        {
            var store = GetStore();

            return store.LoadAll<CatalogFeedItem>();
        }

        public void Delete(Identity id)
        {
            var store = GetStore();

            store.Delete(id);
        }

        private DynamicDataStore GetStore()
        {
            return _dynamicDataStoreFactory.CreateStore(StoreName, typeof(CatalogFeedItem));
            //return DynamicDataStoreFactory.Instance.CreateStore(StoreName, typeof(CatalogFeedItem));
        }
    }
}
