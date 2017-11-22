using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using EPiServer.Commerce.Catalog.ContentTypes;
using Mediachase.Commerce;

namespace StefanOlsen.Commerce.CatalogFeed.GoogleMerchant.AdminPlugin.ViewModels
{
    public class AdministrationViewModel
    {
        public CatalogContent[] AvailableCatalogs { get; set; }

        public IMarket[] AvailableMarkets { get; set; }

        public bool Enabled { get; set; }

        [Required]
        public string FeedName { get; set; }

        [Required]
        [StringLength(32, MinimumLength = 10)]
        public string Key { get; set; }

        public int[] CatalogIds { get; set; }

        public string[] MarketIds { get; set; }

        [Required]
        [Range(10, short.MaxValue)]
        public int FeedExpirationMinutes { get; set; }

        [AllowHtml]
        [Required]
        public string MappingDocument { get; set; }
    }
}
