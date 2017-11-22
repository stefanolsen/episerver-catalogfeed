using System;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Data;
using EPiServer.Data.Dynamic;

namespace StefanOlsen.Commerce.CatalogFeed.Settings
{
    [EPiServerDataStore(AutomaticallyCreateStore = true, AutomaticallyRemapStore = true)]
    public class FeedSettings : IDynamicData
    {
        public FeedSettings()
        {
            Id = Identity.NewIdentity();
            CatalogIds = string.Empty;
            MarketIds = CatalogIds = string.Empty;
            FeedExpirationMinutes = 60 * 24; // 1 day.
            MappingDocument = string.Empty;
        }

        public Identity Id { get; set; }

        [EPiServerDataIndex]
        public string ProviderName { get; set; }

        public bool Enabled { get; set; }

        public string Key { get; set; }

        public string FeedName { get; set; }

        public string CatalogIds { get; set; }

        public string MarketIds { get; set; }

        public int FeedExpirationMinutes { get; set; }

        [AllowHtml]
        public string MappingDocument { get; set; }

        [EPiServerIgnoreDataMember]
        public int[] CatalogIdList
        {
            get => CatalogIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToArray();
            set => CatalogIds = string.Join(",", value.Select(x => x.ToString()));
        }

        [EPiServerIgnoreDataMember]
        public string[] MarketIdList
        {
            get => MarketIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            set => MarketIds = string.Join(",", value);
        }
    }
}
