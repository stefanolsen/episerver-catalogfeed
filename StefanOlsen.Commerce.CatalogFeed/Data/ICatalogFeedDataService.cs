using System;
using System.Collections.Generic;
using EPiServer.Data;

namespace StefanOlsen.Commerce.CatalogFeed.Data
{
    public interface ICatalogFeedDataService
    {
        CatalogFeedItem Create(Uri blobId, string providerName, string marketId);

        void Delete(Identity id);

        CatalogFeedItem Get(string feedItemId);

        CatalogFeedItem Get(string providerName, string marketId);

        IEnumerable<CatalogFeedItem> GetAll();
    }
}