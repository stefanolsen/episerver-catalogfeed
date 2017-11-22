using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace StefanOlsen.Commerce.CatalogFeed.Mapping
{
    public static class FieldMappingHelper
    {
        public static FieldMapping LoadFieldMapping(string mappingDocument)
        {
            if (mappingDocument == null)
            {
                throw new ArgumentNullException(nameof(mappingDocument));
            }

            var serializer = new XmlSerializer(typeof(FieldMapping));
            using (TextReader reader = new StringReader(mappingDocument))
            {
                var fieldmapping = (FieldMapping)serializer.Deserialize(reader);

                return fieldmapping;
            }
        }

        public static bool ValidateFieldMapping(string mappingDocument)
        {
            if (mappingDocument == null)
            {
                throw new ArgumentNullException(nameof(mappingDocument));
            }

            var serializer = new XmlSerializer(typeof(FieldMapping));
            using (TextReader stringReader = new StringReader(mappingDocument))
            using (XmlReader xmlReader = new XmlTextReader(stringReader))
            {
                return serializer.CanDeserialize(xmlReader);
            }
        }
    }
}
