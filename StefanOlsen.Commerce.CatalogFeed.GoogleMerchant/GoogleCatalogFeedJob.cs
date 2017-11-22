using System;
using System.IO;
using System.Linq;
using EPiServer.Framework.Blobs;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using StefanOlsen.Commerce.CatalogFeed.Data;
using StefanOlsen.Commerce.CatalogFeed.Mapping;
using StefanOlsen.Commerce.CatalogFeed.Settings;

namespace StefanOlsen.Commerce.CatalogFeed.GoogleMerchant
{
    [ScheduledPlugIn(DisplayName = "Google Catalog Feed")]
    public class GoogleCatalogFeedJob : ScheduledJobBase
    {
        private static readonly Guid BlobContainerId = Guid.Parse("C01CD7D6-67A2-489B-AEFB-BC28EC583E73");
        private readonly GoogleCatalogFeedService _catalogFeedService;
        private readonly IBlobFactory _blobFactory;
        private readonly IMarketService _marketService;
        private readonly ICatalogFeedDataService _catalogFeedDataService;
        private readonly SettingsRepository _settingsRepository;

        public GoogleCatalogFeedJob(
            GoogleCatalogFeedService catalogFeedService,
            IBlobFactory blobFactory,
            IMarketService marketService,
            ICatalogFeedDataService catalogFeedDataService,
            SettingsRepository settingsRepository)
        {
            _catalogFeedService = catalogFeedService;
            _blobFactory = blobFactory;
            _marketService = marketService;
            _catalogFeedDataService = catalogFeedDataService;
            _settingsRepository = settingsRepository;
        }

        public override string Execute()
        {
            FeedSettings feedSettings = _settingsRepository.GetFeedSettings(Constants.ProviderNameGoogle);
            if (!CanExecute(feedSettings))
            {
                return "This catalog feed is not enabled or not completely set up. Exiting.";
            }

            FieldMapping fieldMapping = FieldMappingHelper.LoadFieldMapping(feedSettings.MappingDocument);

            foreach (var marketId in feedSettings.MarketIdList)
            {
                IMarket market = _marketService.GetMarket(new MarketId(marketId));

                Uri containerIdentifier = Blob.GetContainerIdentifier(BlobContainerId);
                Blob blob = _blobFactory.CreateBlob(containerIdentifier, ".xml");

                using (Stream blobStream = blob.OpenWrite())
                {
                    OnStatusChanged($"Creating catalog feed for market {market.MarketId}.");

                    _catalogFeedService.GetCatalogFeed(feedSettings.CatalogIdList, market, feedSettings.FeedName, fieldMapping, blobStream);

                    OnStatusChanged("Catalog feed created.");
                }

                _catalogFeedDataService.Create(blob.ID, Constants.ProviderNameGoogle, market.MarketId.Value);
            }

            //OnStatusChanged("");

            CleanFeedItems();

            return "Sucessfully created catalog feeds.";
        }

        private bool CanExecute(FeedSettings feedSettings)
        {
            return feedSettings != null &&
                   feedSettings.Enabled &&
                   feedSettings.CatalogIds.Any() &&
                   feedSettings.MarketIdList.Any();
        }

        private void CleanFeedItems()
        {
            int count = 0;

            var feedItems = _catalogFeedDataService.GetAll();
            foreach (var feedItem in feedItems)
            {
                if (feedItem.ExpireDate > DateTime.UtcNow)
                {
                    continue;
                }

                OnStatusChanged($"Deleting expired blob ({feedItem.BlobId}).");

                _blobFactory.Delete(feedItem.BlobId);
                _catalogFeedDataService.Delete(feedItem.Id);
            }

            OnStatusChanged($"Deleted {count} expired feed blobs.");
        }
    }
}
