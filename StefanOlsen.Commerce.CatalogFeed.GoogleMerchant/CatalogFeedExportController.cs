using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using EPiServer.Framework.Blobs;
using StefanOlsen.Commerce.CatalogFeed.Data;
using StefanOlsen.Commerce.CatalogFeed.Settings;

namespace StefanOlsen.Commerce.CatalogFeed.GoogleMerchant
{
    [RoutePrefix("catalogfeed/googlemerchant")]
    public class GoogleMerchantCatalogFeedController : ApiController
    {
        private readonly IBlobFactory _blobFactory;
        private readonly CatalogFeedDataService _catalogFeedDataService;
        private readonly SettingsRepository _settingsRepository;


        public GoogleMerchantCatalogFeedController(
            IBlobFactory blobFactory,
            CatalogFeedDataService catalogFeedDataService,
            SettingsRepository settingsRepository)
        {
            _blobFactory = blobFactory;
            _catalogFeedDataService = catalogFeedDataService;
            _settingsRepository = settingsRepository;
        }

        [Route("")]
        public HttpResponseMessage GetFeed(string key, string marketId)
        {
            FeedSettings feedSettings = _settingsRepository.GetFeedSettings(Constants.ProviderNameGoogle);
            if (feedSettings == null ||
                !feedSettings.Enabled)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            if (!string.Equals(key, feedSettings.Key, StringComparison.InvariantCulture))
            {
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }

            CatalogFeedItem feedItem = _catalogFeedDataService.Get(Constants.ProviderNameGoogle, marketId);
            if (feedItem == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            Blob blob = _blobFactory.GetBlob(feedItem.BlobId);
            return new HttpResponseMessage
            {
                Content = new PushStreamContent(async (outputStream, httpContent, transportContext) =>
                    await WriteToStream(outputStream, blob),
                    new MediaTypeHeaderValue("aplication/xml"))
            };
        }

        private static async Task WriteToStream(Stream outputStream, Blob blob)
        {
            Stream blobStream = blob.OpenRead();
            try
            {
                await blobStream.CopyToAsync(outputStream);
            }
            finally
            {
                blobStream.Dispose();
                outputStream.Dispose();
            }
        }
    }
}
