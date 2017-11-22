using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using StefanOlsen.Commerce.CatalogFeed.GoogleMerchant.Models;
using StefanOlsen.Commerce.CatalogFeed.Mapping;

namespace StefanOlsen.Commerce.CatalogFeed.GoogleMerchant
{
    [ServiceConfiguration(typeof(GoogleCatalogFeedService))]
    public class GoogleCatalogFeedService : ICatalogFeedService
    {
        private readonly CatalogService _catalogService;
        private readonly IMarketService _marketService;
        private readonly IProductDataService _productDataService;
        private readonly ProductMetadataMapper _productMetadataMapper;
        private readonly UrlResolver _urlResolver;
        private FieldMapping _fieldMapping;

        public GoogleCatalogFeedService(
            CatalogService catalogService,
            IMarketService marketService,
            IProductDataService productDataService,
            ProductMetadataMapper productMetadataMapper,
            UrlResolver urlResolver)
        {
            _catalogService = catalogService;
            _marketService = marketService;
            _productDataService = productDataService;
            _productMetadataMapper = productMetadataMapper;
            _urlResolver = urlResolver;
        }

        public void GetCatalogFeed(int[] catalogIds, IMarket market, string feedName, FieldMapping fieldMapping, Stream outputStream)
        {
            _fieldMapping = fieldMapping;

            var feed = new Feed();
            feed.Title = feedName;
            feed.Updated = DateTime.UtcNow;

            IEnumerable<CatalogContent> catalogs = _catalogService.GetCatalogs(catalogIds);
            IEnumerable<Entry> entries = GetEntries(catalogs);
            feed.Entries = new Enumerable<Entry>(entries);

            Serialize(feed, outputStream);
        }

        protected IEnumerable<Entry> GetEntries(IEnumerable<CatalogContent> catalogContents)
        {
            return catalogContents.SelectMany(GetEntries);
        }

        protected IEnumerable<Entry> GetEntries(CatalogContent catalogContent)
        {
            var defaultCulture = CultureInfo.GetCultureInfo(catalogContent.DefaultLanguage);

            var products = _catalogService.GetTreeEntries<ProductContent>(catalogContent.ContentLink, defaultCulture);
            var productEntries = products.SelectMany(p => GetEntries(p, defaultCulture))
                .Where(e => e != null);

            return productEntries;
        }

        protected IEnumerable<Entry> GetEntries(ProductContent productContent, CultureInfo defaultCulture)
        {
            var catalogNode = _catalogService.GetParentCatalogNode(productContent);
            var variations = _catalogService.GetVariations(productContent).ToList();

            IMarket market = _marketService.GetAllMarkets().FirstOrDefault();
            Dictionary<string, ItemPrice> itemPrices = _productDataService
                .GetPrices(variations, market, DateTime.Now)
                .ToDictionary(ip => ip.Code, ip => ip);

            foreach (var variation in variations)
            {
                if (!itemPrices.TryGetValue(variation.Code, out ItemPrice itemPrice))
                {
                    continue;
                }

                Entry entry = GetEntry(catalogNode, productContent, variation, itemPrice, defaultCulture);

                yield return entry;
            }
        }

        protected Entry GetEntry(
            NodeContent nodeContent,
            ProductContent productContent,
            VariationContent variationContent,
            ItemPrice itemPrice,
            CultureInfo defaultCulture)
        {
            var entry = new Entry();
            entry.Id = variationContent.Code;
            entry.Price = $"{itemPrice.UnitPrice:F2} {itemPrice.Currency}";
            entry.SalePrice = $"{itemPrice.SalePrice:F2} {itemPrice.Currency}";

            var nodeMapping =
                _fieldMapping.ContentType.FirstOrDefault(
                    x => x.CommerceType == simpleTypeCommerceEntityType.CatalogNode);
            var productMapping =
                _fieldMapping.ContentType.FirstOrDefault(x => x.CommerceType == simpleTypeCommerceEntityType.Product);
            var variationMapping =
                _fieldMapping.ContentType.FirstOrDefault(x => x.CommerceType == simpleTypeCommerceEntityType.Variation);

            AddLinkUrl(entry, productContent);
            AddImageUrls(entry, productContent, productMapping?.ImageGroup?.AssetMetaField);

            _productMetadataMapper.SetEntryProperties(entry, nodeContent, nodeMapping?.Fields);
            _productMetadataMapper.SetEntryProperties(entry, productContent, productMapping?.Fields);
            _productMetadataMapper.SetEntryProperties(entry, variationContent, variationMapping?.Fields);

            ItemInventory itemInventory = _productDataService.GetInventory(variationContent);
            if (itemInventory == null ||
                itemInventory.AvailableQuantity == 0 &&
                itemInventory.PreorderQuantity == 0)
            {
                entry.Availablity = Constants.InventoryOutOfStock;
            }
            else if (itemInventory.AvailableQuantity > 0)
            {
                entry.Availablity = Constants.InventoryInStock;
            }
            else if (itemInventory.PreorderQuantity > 0)
            {
                entry.Availablity = Constants.InventoryPreorder;
            }

            return entry;
        }

        private void AddLinkUrl(Entry entry, CatalogContentBase entryContent)
        {
            string url = GetUrl(entryContent.ContentLink, entryContent.Language);

            entry.Link = url;
        }

        private void AddImageUrls(Entry entry, ProductContent productContent, string groupName)
        {
            IEnumerable<CommerceMedia> mediaItems = productContent.CommerceMediaCollection;
            mediaItems = mediaItems
                .OrderBy(mi => mi.SortOrder)
                .Where(mi =>
                    string.IsNullOrWhiteSpace(groupName) ||
                    string.Equals(mi.GroupName, groupName, StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

            if (!mediaItems.Any())
            {
                return;
            }

            string[] imageUrls = mediaItems.Select(mi => GetUrl(mi.AssetLink, null)).ToArray();
            entry.ImageLink = imageUrls.First();

            entry.AdditionalImageLinks = imageUrls.Skip(1).ToArray();
        }

        private string GetUrl(ContentReference contentlink, CultureInfo language)
        {
            string url = _urlResolver.GetUrl(
                contentlink,
                language?.Name,
                new VirtualPathArguments
                {
                    ContextMode = ContextMode.Default,
                    ValidateTemplate = false
                });

            url = UriSupport.AbsoluteUrlBySettings(url);

            return url;
        }

        private void Serialize(Feed feed, Stream outputStream)
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", Constants.NamespaceAtom);
            namespaces.Add("g", Constants.NamespaceGoogleMerchant);

            var writerSettings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false,
#if DEBUG
                Indent = true
#endif
            };
            using (StreamWriter streamWriter = new StreamWriter(outputStream, writerSettings.Encoding, 4096, true))
            //using (TextWriter stringWriter = new StringWriter())
            using (XmlWriter xmlWriter = XmlWriter.Create(streamWriter, writerSettings))
            {
                var serializer = new XmlSerializer(typeof(Feed));
                serializer.Serialize(xmlWriter, feed, namespaces);
            }
        }
    }
}
