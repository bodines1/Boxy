using Boxy.Model.ScryfallData;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Net;
using System.Threading.Tasks;

namespace Boxy.Model
{
    public static class ScryfallService
    {
        #region Services

        public static async Task<Card> GetCardsAsync(string search)
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
                using (var webClient = new WebClient())
                {
                    var request = EndPoint + GetNamedCardUri + search;
                    var json = await webClient.DownloadStringTaskAsync(request);
                    return JsonConvert.DeserializeObject<Card>(json);
                }
            }
            catch (WebException)
            {
                return null;
            }
        }

        public static async Task<Bitmap> GetBorderCropImageAsync(Card card)
        {
            // Can't find image without a valid card.
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card), "Card object cannot be null. Consumer must check card before using this method.");
            }

            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = await client.OpenReadTaskAsync(card.ImageUris.BorderCrop))
                    {
                        var bitmap = new Bitmap(stream ?? throw new InvalidOperationException("File stream from service was null, ensure the URI is correct."));
                        await stream.FlushAsync();
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
