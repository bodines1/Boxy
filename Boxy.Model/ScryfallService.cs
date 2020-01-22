﻿using Boxy.Model.ScryfallData;
using Boxy.Resources.Reporting;
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

        public static async Task<Card> GetFuzzyCardAsync(string search, IReporter reporter)
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
                reporter.Report($"Searching Scryfall for [{search}]");
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

        public static async Task<List<Card>> GetAllPrintingsAsync(string oracleId, IReporter reporter)
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
                    reporter.Report("Downloading alternate prints from Scryfall");
                    var result = new List<Card>();
                    string request = ExactCardSearchWithPrintings + oracleId;
                    string json = await webClient.DownloadStringTaskAsync(request);
                    var scryfallList = JsonConvert.DeserializeObject<ScryfallList<Card>>(json);
                    result.AddRange(scryfallList.Data);

                    while (scryfallList.HasMore)
                    {
                        json = await webClient.DownloadStringTaskAsync(scryfallList.NextPage);
                        scryfallList = JsonConvert.DeserializeObject<ScryfallList<Card>>(json);
                        result.AddRange(scryfallList.Data);
                    }

                    return result;
                }
            }
            catch (WebException)
            {
                return null;
            }
        }

        public static async Task<Bitmap> GetBorderCropImageAsync(Card card, IReporter reporter)
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
                        reporter.Report($"Downloading [{card.Name}] art from Scryfall");
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

        public static async Task<ScryfallList<BulkData>> GetBulkDataInfo(IReporter reporter)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    reporter.Report("Downloading bulk card metadata from Scryfall");
                    string json = await webClient.DownloadStringTaskAsync(BulkData);
                    return JsonConvert.DeserializeObject<ScryfallList<BulkData>>(json);
                }
            }
            catch (WebException)
            {
                return null;
            }
        }

        public static async Task<List<Card>> GetBulkCards(Uri catalogUri, IReporter reporter)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    reporter.Report("Downloading card catalog from Scryfall");
                    string json = await webClient.DownloadStringTaskAsync(catalogUri);
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
        private static Uri ExactCardSearchWithPrintings { get; } = new Uri("https://api.scryfall.com/cards/search?order=released&dir=auto&unique=prints&q=oracle_id%3A");

        #endregion URIs
    }
}
