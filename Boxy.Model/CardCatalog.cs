using Boxy.Model.ScryfallData;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace Boxy.Model
{
    /// <summary>
    /// Holds a queryable set of all oracle cards (no duplicate printings) to prevent excess queries to ScryfallAPI.
    /// </summary>
    public class CardCatalog
    {
        /// <summary>
        /// Creates a new instance of this class from code. Used to create a new catalog, but normal operation will have this created from deserializing a local file.
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="cards"></param>
        public CardCatalog(BulkData metadata, List<Card> cards)
        {
            Metadata = metadata;
            Cards = cards;
        }

        /// <summary>
        /// Direct permalink URI to download the card data.
        /// </summary>
        public static Uri ScryfallUri { get; } = new Uri("https://archive.scryfall.com/json/scryfall-oracle-cards.json");

        /// <summary>
        /// Where to save the local serialized copy of this catalog.
        /// </summary>
        public static string SavePath { get; } = @"scryfall-oracle-cards.json";

        /// <summary>
        /// Metadata information about the catalog.
        /// </summary>
        public BulkData Metadata { get; }

        /// <summary>
        /// The queryable card collection.
        /// </summary>
        public List<Card> Cards { get; }

        /// <summary>
        /// Find a specific card with an exact name search.
        /// </summary>
        /// <param name="name">The card name to search for.</param>
        /// <returns>The card object with the matching</returns>
        public Card FindExactCard(string name)
        {
            return Cards.Find(c => c.Name.ToUpper(CultureInfo.CurrentCulture).Trim() == name.ToUpper(CultureInfo.CurrentCulture).Trim());
        }
    }
}
