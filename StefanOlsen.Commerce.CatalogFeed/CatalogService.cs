using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using Mediachase.Commerce.Catalog;

namespace StefanOlsen.Commerce.CatalogFeed
{
    public class CatalogService
    {
        private readonly IContentLoader _contentLoader;
        private readonly IPublishedStateAssessor _publishedStateAssessor;
        private readonly ReferenceConverter _referenceConverter;

        public CatalogService(
            IContentLoader contentLoader,
            IPublishedStateAssessor publishedStateAssessor,
            ReferenceConverter referenceConverter)
        {
            _contentLoader = contentLoader;
            _publishedStateAssessor = publishedStateAssessor;
            _referenceConverter = referenceConverter;
        }

        public virtual IEnumerable<CatalogContent> GetCatalogs(int[] catalogIds)
        {
            ContentReference catalogRoot = _referenceConverter.GetRootLink();
            IEnumerable<CatalogContent> catalogs = _contentLoader.GetChildren<CatalogContent>(catalogRoot);

            return catalogs.Where(c => catalogIds.Contains(c.CatalogId));
        }

        public virtual IEnumerable<CatalogContent> GetCatalogs()
        {
            ContentReference catalogRoot = _referenceConverter.GetRootLink();
            IEnumerable<CatalogContent> catalogs = _contentLoader.GetChildren<CatalogContent>(catalogRoot);

            return catalogs;
        }

        public virtual NodeContent GetParentCatalogNode(ProductContent product)
        {
            ContentReference parentLink = product.ParentLink;
            if (ContentReference.IsNullOrEmpty(parentLink))
            {
                return null;
            }

            CatalogContentType contentType = _referenceConverter.GetContentType(parentLink);
            if (contentType != CatalogContentType.CatalogNode)
            {
                return null;
            }

            bool exists = _contentLoader.TryGet(parentLink, product.Language, out NodeContent nodeContent);

            return exists ? nodeContent : null;
        }

        public virtual IEnumerable<VariationContent> GetVariations(ProductContent product)
        {
            IEnumerable<VariationContent> variations = _contentLoader
                .GetItems(product.GetVariants(), product.Language)
                .OfType<VariationContent>()
                .Where(c => _publishedStateAssessor.IsPublished(c));

            return variations;
        }

        public virtual IEnumerable<TContent> GetTreeEntries<TContent>(ContentReference parentLink, CultureInfo defaultCulture)
            where TContent : EntryContentBase
        {
            var childNodes = _contentLoader.GetChildren<NodeContent>(parentLink, defaultCulture);
            foreach (NodeContent childNode in childNodes)
            {
                foreach (var entry in GetTreeEntries<TContent>(childNode.ContentLink, defaultCulture))
                {
                    yield return entry;
                }
            }

            var childEntries = _contentLoader.GetChildren<TContent>(parentLink, defaultCulture);
            foreach (var childEntry in childEntries)
            {
                yield return childEntry;
            }
        }
    }
}
