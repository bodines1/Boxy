using CardMimic.Model.ScryfallData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CardMimic.Model
{
    public static class ScryfallService
    {
        #region Services

        public static async Task<Card> GetFuzzyCardAsync(string search, IProgress<string> reporter)
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

        public static async Task<List<Card>> GetAllPrintingsAsync(Card card, IProgress<string> reporter)
        {
            // Return nothing if search value is meaningless.
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card), "Card object cannot be null. Consumer must check card before using this method.");
            }

            try
            {
                using (var webClient = new WebClient())
                {
                    var result = new List<Card>();

                    string json = await webClient.DownloadStringTaskAsync(ExactCardSearchWithPrintings + card.OracleId);
                    var scryfallList = JsonConvert.DeserializeObject<ScryfallList<Card>>(json);
                    result.AddRange(scryfallList.Data);

                    while (scryfallList.HasMore)
                    {
                        json = await webClient.DownloadStringTaskAsync(scryfallList.NextPage);
                        scryfallList = JsonConvert.DeserializeObject<ScryfallList<Card>>(json);
                        result.AddRange(scryfallList.Data);
                    }

                    result.RemoveAll(crd => crd.CollectorNumber.Any(ch => ch == 's' || ch == 'p' || crd.Digital));

                    return result;
                }
            }
            catch (WebException)
            {
                return null;
            }
        }

        // ReSharper disable once UnusedParameter.Global <-- I think I may want the reporter later, but it is safe to remove
        public static async Task<Bitmap> GetImageAsync(string imageUri, IProgress<string> reporter)
        {
            // Can't find image without a valid card.
            if (string.IsNullOrWhiteSpace(imageUri))
            {
                throw new ArgumentNullException(nameof(imageUri), "Image request URI cannot be null or empty/whitespace. Consumer must check before using this method.");
            }

            try
            {
                using (var client = new WebClient())
                {
                    using (Stream stream = await client.OpenReadTaskAsync(imageUri))
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

        public static async Task<ScryfallList<BulkData>> GetBulkDataInfo(IProgress<string> reporter)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    string json = await webClient.DownloadStringTaskAsync(BulkData);
                    return JsonConvert.DeserializeObject<ScryfallList<BulkData>>(json);
                }
            }
            catch (WebException)
            {
                return null;
            }
        }

        public static async Task<List<Card>> GetBulkCards(Uri catalogUri, IProgress<string> reporter)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    string json = await webClient.DownloadStringTaskAsync(catalogUri);
                    return JsonConvert.DeserializeObject<List<Card>>(json);
                }
            }
            catch (WebException)
            {
                return null;
            }
        }

        public static async Task<Card> GetRandomCard(IProgress<string> reporter)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    string name = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
                    reporter.Report(name);
                    string json = await webClient.DownloadStringTaskAsync(RandomCard);
                    return JsonConvert.DeserializeObject<Card>(json);
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
        /// Returns <see cref="Card"/>.
        /// </summary>
        private static Uri FuzzyCardSearch { get; } = new Uri("https://api.scryfall.com/cards/named?fuzzy=");

        /// <summary>
        /// Returns <see cref="Card"/>.
        /// </summary>
        private static Uri BulkData { get; } = new Uri("https://api.scryfall.com/bulk-data");
        
        /// <summary>
        /// Returns <see cref="ScryfallList{T}"/> where data is <see cref="Card"/> objects.
        /// </summary>
        private static Uri ExactCardSearchWithPrintings { get; } = new Uri("https://api.scryfall.com/cards/search?order=released&unique=prints&q=digital%3Afalse+oracle_id%3A");
        
        /// <summary>
        /// Returns a random <see cref="Card"/> from Scryfall.
        /// </summary>
        private static Uri RandomCard { get; } = new Uri("https://api.scryfall.com/cards/random?");

        #endregion URIs
    }
}