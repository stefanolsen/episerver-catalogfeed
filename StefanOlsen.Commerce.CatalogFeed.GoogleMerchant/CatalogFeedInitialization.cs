using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using StefanOlsen.Commerce.CatalogFeed.Data;

namespace StefanOlsen.Commerce.CatalogFeed.GoogleMerchant
{
    [InitializableModule]
    [ModuleDependency(typeof(ServiceContainerInitialization))]
    public class CatalogFeedInitialization : IConfigurableModule
    {
        public void Initialize(InitializationEngine context)
        {
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            IServiceConfigurationProvider services = context.Services;

            services.AddTransient<ICatalogFeedDataService, CatalogFeedDataService>();
            services.AddTransient<IProductDataService, ProductDataService>();
        }
    }
}
