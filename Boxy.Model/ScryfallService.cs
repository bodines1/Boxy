using Boxy.Model.ScryfallData;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Net;

namespace Boxy.Model
{
    public static class ScryfallService
    {
        #region Services

        public static Card GetCards(string search)
        {
            // Return nothing if search value is meaningless.
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            // The API expects search terms to be separated by the + symbol in place of whitespace.
            var searchTerms = search.Trim().Split();
            search = string.Join("+", searchTerms);

            try
            {
                using (var webClient = new WebClient { BaseAddress = EndPoint })
                {
                    var request = GetNamedCardUri + search;
                    var json = webClient.DownloadString(request);
                    return JsonConvert.DeserializeObject<Card>(json);
                }
            }
            catch (WebException)
            {
                return null;
            }
        }

        public static Bitmap GetBorderCropImage(Card card)
        {
            // Can't find image without a valid card.
            if (card is null)
            {
                return null;
            }

            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead(card.ImageUris.BorderCrop))
                    {
                        var bitmap = new Bitmap(stream ?? throw new InvalidOperationException("File stream from service was null, ensure the URI is correct."));
                        stream.Flush();
                        return bitmap;
                    }
                }
            }
            catch (WebException)
            {
                return null;
            }
        }

        #endregion Services

        #region URIs

        public static string EndPoint { get; } = "https://api.scryfall.com/";

        public static string CardNamesCatalogUri { get; } = "catalog/card-names";

        public static string GetBulkDataUri { get; } = "bulk-data";

        public static string GetNamedCardUri { get; } = "cards/named?fuzzy=";

        #endregion URIs
    }
}
