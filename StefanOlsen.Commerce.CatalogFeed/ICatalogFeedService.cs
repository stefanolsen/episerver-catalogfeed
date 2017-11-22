using System.IO;
using Mediachase.Commerce;
using StefanOlsen.Commerce.CatalogFeed.Mapping;

namespace StefanOlsen.Commerce.CatalogFeed
{
    public interface ICatalogFeedService
    {
        void GetCatalogFeed(int[] catalogIds, IMarket market, string feedName, FieldMapping fieldMapping, Stream outputStream);
    }
}
