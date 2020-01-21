using Boxy.Model.ScryfallData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Boxy.Model
{
    public static class ScryfallService
    {
        #region Services

        public static async Task<Card> GetFuzzyCardAsync(string search)
        {
            // Return nothing if search value is meaningless.
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            // The API expects search terms to be separated by the + symbol in place of whitespace.
            string[] searchTerms = search.Trim().Split();
            search = string.Join("+", searchTerms);

            try
            {
                using (var webClient = new WebClient())
                {
                    string request = FuzzyCardSearch + search;
                    string json = await webClient.DownloadStringTaskAsync(request);
                    return JsonConvert.DeserializeObject<Card>(json);
                }
            }
            catch (WebException)
            {
                return null;
            }
        }

        public static async Task<CardList> GetExactCardWithPrintingsAsync(string oracleId)
        {
            // Return nothing if search value is meaningless.
            if (string.IsNullOrWhiteSpace(oracleId))
            {
                return null;
            }

            try
            {
                using (var webClient = new WebClient())
                {
                    string request = ExactCardSearchWithPrintings + oracleId;
                    string json = await webClient.DownloadStringTaskAsync(request);
                    var result = JsonConvert.DeserializeObject<CardList>(json);
                    return result;
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
                    using (Stream stream = await client.OpenReadTaskAsync(card.ImageUris.BorderCrop))
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

        public static async Task<List<Card>> GetBulkDataCatalog()
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    string json = await webClient.DownloadStringTaskAsync(OracleCardCatalog);
                    return JsonConvert.DeserializeObject<List<Card>>(json);
                }
            }
            catch (WebException)
            {
                return null;
            }
        }

        #endregion Services

        #region URIs

        /// <summary>
        /// API root location.
        /// </summary>
        public static Uri ApiRoot { get; } = new Uri("https://api.scryfall.com/");

        /// <summary>
        /// Returns <see cref="ScryfallList"/> where data is <see cref="Card"/> objects.
        /// </summary>
        public static Uri OracleCardCatalog { get; } = new Uri("https://archive.scryfall.com/json/scryfall-oracle-cards.json");

        /// <summary>
        /// Returns <see cref="Card"/>.
        /// </summary>
        public static Uri FuzzyCardSearch { get; } = new Uri("https://api.scryfall.com/cards/named?fuzzy=");

        /// <summary>
        /// Returns <see cref="ScryfallList"/> where data is <see cref="Card"/> objects.
        /// </summary>
        public static Uri ExactCardSearchWithPrintings { get; } = new Uri("https://api.scryfall.com/cards/search?order=released&dir=auto&unique=art&q=oracle_id%3A");

        #endregion URIs
    }
}
