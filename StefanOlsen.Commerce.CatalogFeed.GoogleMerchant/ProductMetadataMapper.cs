using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.HtmlParsing;
using EPiServer.Security;
using Mediachase.MetaDataPlus;
using Mediachase.MetaDataPlus.Configurator;
using StefanOlsen.Commerce.CatalogFeed.Mapping;
using Entry = StefanOlsen.Commerce.CatalogFeed.GoogleMerchant.Models.Entry;

namespace StefanOlsen.Commerce.CatalogFeed.GoogleMerchant
{
    public class ProductMetadataMapper
    {
        private readonly Dictionary<int, MetaField[]> _metaClassFields;

        public ProductMetadataMapper()
        {
            _metaClassFields = new Dictionary<int, MetaField[]>();            
        }

        public void SetEntryProperties<TContent>(Entry entry, TContent entryContent, ICollection<complexTypeBaseFieldType> fieldMappings)
            where TContent : ContentData, IMetaClass, ILocale
        {
            if (entry == null ||
                entryContent == null ||
                fieldMappings== null ||
                !fieldMappings.Any())
            {
                return;
            }

            MetaField[] metaFields = GetMetaFields(entryContent, entryContent.Language).ToArray();

            foreach (var fieldMapping in fieldMappings)
            {
                object fieldData;
                if (fieldMapping is complexTypeFixedFieldType)
                {
                    var fixedFieldMapping = fieldMapping as complexTypeFixedFieldType;
                    fieldData = fixedFieldMapping.Value;

                    SetEntryProperties(entry, fieldData, fixedFieldMapping);
                }
                else if (fieldMapping is complexTypeMappedFieldType)
                {
                    var mappedFieldMapping = fieldMapping as complexTypeMappedFieldType;

                    MetaField metaField = metaFields
                        .FirstOrDefault(mf => mf.Name == mappedFieldMapping.MetaField);
                    if (metaField == null)
                    {
                        continue;
                    }

                    fieldData = entryContent[mappedFieldMapping.MetaField];

                    SetEntryProperties(entry, fieldData, mappedFieldMapping);
                }
            }
        }

        private void SetEntryProperties(Entry entry, object fieldData, complexTypeBaseFieldType fieldMapping)
        {
            switch (fieldMapping.FeedField)
            {
                case "id":
                    entry.Id = GetStringValue(fieldData);
                    break;
                case "title":
                    entry.Title = GetStringValue(fieldData);
                    break;
                case "description":
                    entry.Description = GetStringValue(fieldData);
                    break;

                case "availability_date":
                    entry.AvailabilityDate = GetDateTimeValue(fieldData);
                    entry.AvailabilityDateSpecified = fieldData is DateTime;
                    break;
                case "expiration_date":
                    entry.ExpirationDate = GetDateTimeValue(fieldData);
                    entry.ExpirationDateSpecified = fieldData is DateTime;
                    break;

                case "google_product_category":
                    entry.GoogleProductCategory = GetIntValue(fieldData);
                    entry.GoogleProductCategorySpecified = fieldData is int;
                    break;
                case "product_type":
                    entry.ProductType = GetStringValue(fieldData);
                    break;

                case "brand":
                    entry.Brand = GetStringValue(fieldData);
                    break;
                case "gtin":
                    entry.Gtin = GetStringValue(fieldData);
                    break;
                case "mpn":
                    entry.MPN = GetStringValue(fieldData);
                    break;

                case "condition":
                    entry.Condition = GetStringValue(fieldData);
                    break;
                case "is_adult":
                    entry.IsAdult = GetBooleanValue(fieldData);
                    entry.IsAdultSpecified = fieldData is bool;
                    break;
                case "age_group":
                    entry.AgeGroup = GetStringValue(fieldData);
                    break;
                case "color":
                    entry.Color = GetStringValue(fieldData);
                    break;
                case "gender":
                    entry.Gender = GetStringValue(fieldData);
                    break;
                case "size":
                    entry.Size = GetStringValue(fieldData);
                    break;
                case "size_system":
                    entry.Size = GetStringValue(fieldData);
                    break;

                case "link":
                case "image_link":
                case "additional_image_link":
                case "availabiity":
                case "price":
                case "sale_price":
                case "is_bundle":
                    // Do nothing. These should be calculated by code.
                    break;
            }
        }

        private static DateTime GetDateTimeValue(object fieldData)
        {
            if (fieldData == null)
            {
                return default(DateTime);
            }

            return (DateTime)fieldData;
        }

        private static bool GetBooleanValue(object fieldData)
        {
            if (fieldData == null)
            {
                return false;
            }

            return (bool)fieldData;
        }

        private static int GetIntValue(object fieldData)
        {
            if (fieldData == null)
            {
                return default(int);
            }

            return (int) fieldData;
        }

        private static string GetStringValue(object fieldData)
        {
            if (fieldData is XhtmlString xhtmlString)
            {
                string htmlString = xhtmlString.ToHtmlString(PrincipalInfo.AnonymousPrincipal);
                string textString = StripHtml(htmlString);

                return textString;
            }

            return (string)fieldData;
        }

        private static string StripHtml(string text)
        {
            IEnumerable<HtmlFragment> fragments = new HtmlStreamReader(text, ParserOptions.None);

            string result = fragments
                .Where(f => f.FragmentType == HtmlFragmentType.Text)
                .Aggregate(string.Empty, (current, source) => current + source);

            return result;
        }

        private IEnumerable<MetaField> GetMetaFields(IMetaClass entryContent, CultureInfo language)
        {
            int metaClassId = entryContent.MetaClassId;
            if (_metaClassFields.TryGetValue(metaClassId, out var metaFields))
            {
                return metaFields;
            }

            var metaClassContext = new MetaDataContext
            {
                UseCurrentThreadCulture = false,
                Language = language.Name
            };

            var metaClass = MetaClass.Load(metaClassContext, metaClassId);
            metaFields = metaClass.GetAllMetaFields().ToArray();

            _metaClassFields.Add(metaClassId, metaFields);

            return metaFields;
        }
    }
}
