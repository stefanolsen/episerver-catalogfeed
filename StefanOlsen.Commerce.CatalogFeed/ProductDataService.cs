using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Marketing;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.InventoryService;
using Mediachase.Commerce.Pricing;

namespace StefanOlsen.Commerce.CatalogFeed
{
    public class ProductDataService : IProductDataService
    {
        private readonly IInventoryService _inventoryService;
        private readonly IPriceService _priceService;
        private readonly IPromotionEngine _promotionEngine;

        public ProductDataService(
            IPriceService priceService,
            IPromotionEngine promotionEngine,
            IInventoryService inventoryService)
        {
            _priceService = priceService;
            _promotionEngine = promotionEngine;
            _inventoryService = inventoryService;
        }

        public IEnumerable<ItemPrice> GetPrices(
            IEnumerable<EntryContentBase> entries,
            IMarket market,
            DateTime validOn)
        {
            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            var catalogKeys = entries
                .Where(e => e is IPricing)
                .ToDictionary(e => new CatalogKey(e.Code), e => e);

            var priceFilter = new PriceFilter
            {
                Currencies = market.Currencies,
                CustomerPricing = new[] { CustomerPricing.AllCustomers },
                Quantity = 0M,
                ReturnCustomerPricing = false
            };

            var priceValues = _priceService.GetPrices(market.MarketId, validOn, catalogKeys.Keys, priceFilter).ToList();
            priceValues = priceValues.GroupBy(pv => new { pv.CatalogKey, pv.UnitPrice.Currency })
                .Select(g => g.OrderBy(pv => pv.UnitPrice.Amount).First()).ToList();

            foreach (var priceValue in priceValues)
            {
                if (!catalogKeys.TryGetValue(priceValue.CatalogKey, out EntryContentBase entry))
                {
                    continue;
                }

                DiscountPrice discountPrice = _promotionEngine
                    .GetDiscountPrices(entry.ContentLink, market, priceValue.UnitPrice.Currency)
                    .SelectMany(dp => dp.DiscountPrices)
                    .OrderBy(dp => dp.DefaultPrice.Amount)
                    .FirstOrDefault();

                var itemPrice = new ItemPrice
                {
                    Code = priceValue.CatalogKey.CatalogEntryCode,
                    Currency = priceValue.UnitPrice.Currency,
                    UnitPrice = priceValue.UnitPrice.Amount,
                    SalePrice = discountPrice?.Price.Amount ?? priceValue.UnitPrice.Amount
                };

                yield return itemPrice;
            }
        }

        public ItemInventory GetInventory(EntryContentBase entryContent)
        {
            IList<InventoryRecord> inventoryRecords = _inventoryService.QueryByEntry(new[] {entryContent.Code});
            if (inventoryRecords == null)
            {
                return null;
            }

            decimal availableQuantity = inventoryRecords.Sum(r => r.PurchaseAvailableQuantity);
            decimal preorderQuantity = inventoryRecords.Sum(r => r.PreorderAvailableQuantity);

            var itemInventory = new ItemInventory
            {
                Code = entryContent.Code,
                AvailableQuantity = availableQuantity,
                PreorderQuantity = preorderQuantity
            };

            return itemInventory;
        }
    }
}
