using System.Linq;
using System.Web.Mvc;
using EPiServer.Commerce.Security;
using EPiServer.PlugIn;
using Mediachase.Commerce.Markets;
using StefanOlsen.Commerce.CatalogFeed.GoogleMerchant.AdminPlugin.ViewModels;
using StefanOlsen.Commerce.CatalogFeed.Mapping;
using StefanOlsen.Commerce.CatalogFeed.Settings;

namespace StefanOlsen.Commerce.CatalogFeed.GoogleMerchant.AdminPlugin.Controllers
{
    [GuiPlugIn(
        Area = PlugInArea.AdminConfigMenu,
        DisplayName = "Google Product Feed",
        Url = "/googleproductfeedconfig")]
    [Authorize(Roles = RoleNames.CommerceAdmins)]
    public class GoogleProductFeedConfigController : Controller
    {
        private readonly CatalogService _catalogService;
        private readonly IMarketService _marketService;
        private readonly SettingsRepository _settingsRepository;

        public GoogleProductFeedConfigController(
            CatalogService catalogService,
            IMarketService marketService,
            SettingsRepository settingsRepository)
        {
            _catalogService = catalogService;
            _marketService = marketService;
            _settingsRepository = settingsRepository;
        }

        public ActionResult Index()
        {
            var viewModel = new AdministrationViewModel();
            viewModel.AvailableCatalogs = _catalogService.GetCatalogs().ToArray();
            viewModel.AvailableMarkets = _marketService.GetAllMarkets().ToArray();

            FeedSettings feedSettings =
                _settingsRepository.GetFeedSettings(Constants.ProviderNameGoogle) ?? new FeedSettings();

            viewModel.CatalogIds = feedSettings.CatalogIdList;
            viewModel.MarketIds = feedSettings.MarketIdList;
            viewModel.Enabled = feedSettings.Enabled;
            viewModel.Key = feedSettings.Key;
            viewModel.FeedName = feedSettings.FeedName;
            viewModel.FeedExpirationMinutes = feedSettings.FeedExpirationMinutes;
            viewModel.MappingDocument = feedSettings.MappingDocument;

            return View("Index", viewModel);
        }

        [HttpPost]
        public ActionResult Index(AdministrationViewModel viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel.MappingDocument))
            {
                bool valid = FieldMappingHelper.ValidateFieldMapping(viewModel.MappingDocument);
                if (!valid)
                {
                    ModelState.AddModelError("MappingDocument", "The entered data is not valid XML.");
                }
            }

            if (!ModelState.IsValid)
            {
                return Index();
            }

            FeedSettings feedSettings =
                _settingsRepository.GetFeedSettings(Constants.ProviderNameGoogle) ?? new FeedSettings();

            feedSettings.CatalogIdList = viewModel.CatalogIds;
            feedSettings.MarketIdList = viewModel.MarketIds;
            feedSettings.ProviderName = Constants.ProviderNameGoogle;
            feedSettings.Enabled = viewModel.Enabled;
            feedSettings.Key = viewModel.Key;
            feedSettings.FeedName = viewModel.FeedName;
            feedSettings.FeedExpirationMinutes = viewModel.FeedExpirationMinutes;
            feedSettings.MappingDocument = viewModel.MappingDocument;

            _settingsRepository.Save(feedSettings);

            return Index();
        }
    }
}
