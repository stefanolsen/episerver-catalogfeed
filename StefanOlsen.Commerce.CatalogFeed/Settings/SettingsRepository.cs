using System;
using System.Linq;
using EPiServer.Data.Dynamic;

namespace StefanOlsen.Commerce.CatalogFeed.Settings
{
    public class SettingsRepository
    {
        private readonly DynamicDataStoreFactory _dynamicDataStoreFactory;

        public SettingsRepository(DynamicDataStoreFactory dynamicDataStoreFactory)
        {
            _dynamicDataStoreFactory = dynamicDataStoreFactory;
        }

        public FeedSettings GetFeedSettings(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                throw new ArgumentException("Null or empty value is not allowed.", nameof(providerName));
            }

            var store = GetStore();

            var settings = store.Find<FeedSettings>("ProviderName", providerName).FirstOrDefault();

            return settings;
        }

        public void Save(FeedSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var store = GetStore();

            store.Save(settings, settings.Id);
        }

        private DynamicDataStore GetStore()
        {
            return _dynamicDataStoreFactory.CreateStore(typeof(FeedSettings));
        }
    }
}

