using System;
using System.Xml.Serialization;

namespace StefanOlsen.Commerce.CatalogFeed.GoogleMerchant.Models
{
    [Serializable]
    [XmlType("entry", Namespace = Constants.NamespaceGoogleMerchant)]
    public class Entry
    {
        #region Basic product data
        [XmlElement("id", Namespace = Constants.NamespaceGoogleMerchant)]
        public string Id { get; set; }

        [XmlElement("title", Namespace = Constants.NamespaceGoogleMerchant)]
        public string Title { get; set; }

        [XmlElement("description", Namespace = Constants.NamespaceGoogleMerchant)]
        public string Description { get; set; }

        [XmlElement("link", Namespace = Constants.NamespaceGoogleMerchant)]
        public string Link { get; set; }

        [XmlElement("image_link", Namespace = Constants.NamespaceGoogleMerchant)]
        public string ImageLink { get; set; }

        [XmlElement("additional_image_link")]
        public string[] AdditionalImageLinks { get; set; }
        #endregion

        #region Price & Availability
        [XmlElement("availability", Namespace = Constants.NamespaceGoogleMerchant)]
        public string Availablity { get; set; }

        [XmlElement("availability_date", Namespace = Constants.NamespaceGoogleMerchant)]
        public DateTime AvailabilityDate { get; set; }

        [XmlIgnore]
        public bool AvailabilityDateSpecified { get; set; }

        [XmlElement("expiration_date", Namespace = Constants.NamespaceGoogleMerchant)]
        public DateTime ExpirationDate { get; set; }

        [XmlIgnore]
        public bool ExpirationDateSpecified { get; set; }

        [XmlElement("price", Namespace = Constants.NamespaceGoogleMerchant)]
        public string Price { get; set; }

        [XmlElement("sale_price", Namespace = Constants.NamespaceGoogleMerchant)]
        public string SalePrice { get; set; }

        [XmlElement("shipping", Namespace = Constants.NamespaceGoogleMerchant)]
        public Shipping[] Shipping { get; set; }

        #endregion

        #region Product category
        [XmlElement("google_product_category", Namespace = Constants.NamespaceGoogleMerchant)]
        public int GoogleProductCategory { get; set; }

        [XmlIgnore]
        public bool GoogleProductCategorySpecified { get; set; }

        [XmlElement("product_type", Namespace = Constants.NamespaceGoogleMerchant)]
        public string ProductType { get; set; }
        #endregion

        #region Product IDs
        [XmlElement("brand", Namespace = Constants.NamespaceGoogleMerchant)]
        public string Brand { get; set; }

        [XmlElement("gtin", Namespace = Constants.NamespaceGoogleMerchant)]
        public string Gtin { get; set; }

        [XmlElement("mpn", Namespace = Constants.NamespaceGoogleMerchant)]
        public string MPN { get; set; }

        [XmlElement("identifier_exists", Namespace = Constants.NamespaceGoogleMerchant)]
        public string IdentifierExists =>
            !string.IsNullOrWhiteSpace(Brand) &&
            (!string.IsNullOrWhiteSpace(Gtin) || !string.IsNullOrWhiteSpace(MPN))
                ? Constants.BooleanValueYes
                : Constants.BooleanValueNo;

        [XmlIgnore]
        public bool IdentifierExistsSpecified => IdentifierExists == Constants.BooleanValueNo;
        #endregion

        #region Detailed product description
        [XmlElement("condition", Namespace = Constants.NamespaceGoogleMerchant)]
        public string Condition { get; set; }

        [XmlElement("adult", Namespace = Constants.NamespaceGoogleMerchant)]
        public bool IsAdult { get; set; }

        [XmlIgnore]
        public bool IsAdultSpecified { get; set; }

        [XmlElement("age_group", Namespace = Constants.NamespaceGoogleMerchant)]
        public string AgeGroup { get; set; }

        [XmlElement("color", Namespace = Constants.NamespaceGoogleMerchant)]
        public string Color { get; set; }

        [XmlElement("gender", Namespace = Constants.NamespaceGoogleMerchant)]
        public string Gender { get; set; }

        [XmlElement("size", Namespace = Constants.NamespaceGoogleMerchant)]
        public string Size { get; set; }

        [XmlElement("size_system", Namespace = Constants.NamespaceGoogleMerchant)]
        public string SizeSystem { get; set; }

        [XmlElement("is_bundle", Namespace = Constants.NamespaceGoogleMerchant)]
        public bool IsBundle { get; set; }

        [XmlIgnore]
        public bool IsBundleSpecified { get; set; }
        #endregion
    }
}
