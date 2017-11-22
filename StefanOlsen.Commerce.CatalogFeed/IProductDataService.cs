using System;
using System.Collections.Generic;
using EPiServer.Commerce.Catalog.ContentTypes;
using Mediachase.Commerce;

namespace StefanOlsen.Commerce.CatalogFeed
{
    public interface IProductDataService
    {
        ItemInventory GetInventory(EntryContentBase entryContent);

        IEnumerable<ItemPrice> GetPrices(IEnumerable<EntryContentBase> entries, IMarket market, DateTime validOn);
    }
}