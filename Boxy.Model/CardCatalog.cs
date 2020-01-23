using Boxy.Model.ScryfallData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Boxy.Model
{
    /// <summary>
    /// Holds a queryable set of all oracle cards (no duplicate printings) to prevent excess queries to ScryfallAPI.
    /// </summary>
    public class CardCatalog
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of this class from code. Used to create a new catalog, but normal operation will have this created
        /// from deserializing a local file using <see cref="CreateFromFile"/>
        /// </summary>
        public CardCatalog(BulkData metadata, List<Card> cards)
        {
            Metadata = metadata;
            Cards = cards;
        }

        /// <summary>
        /// Creates an instance of <see cref="ArtworkPreferences"/> by deserializing a file at the <see cref="SavePath"/> if it
        /// exists, or a new instance if deserialization fails.
        /// </summary>
        public static CardCatalog CreateFromFile()
        {
            try
            {
                return JsonConvert.DeserializeObject<CardCatalog>(File.ReadAllText(SavePath));
            }
            catch (Exception)
            {
                return new CardCatalog(new BulkData(), new List<Card>());
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The queryable card collection.
        /// </summary>
        private List<Card> Cards { get; }

        /// <summary>
        /// Where to save the local serialized copy of this catalog.
        /// </summary>
        public static string SavePath { get; } = @"scryfall-oracle-cards.json";

        /// <summary>
        /// Metadata information about the catalog.
        /// </summary>
        public BulkData Metadata { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Find a specific card with an exact name search.
        /// </summary>
        /// <param name="name">The card name to search for.</param>
        /// <returns>The card object with the matching</returns>
        public Card FindExactCard(string name)
        {
            return Cards.Find(c => c.Name.ToUpper(CultureInfo.CurrentCulture).Trim() == name.ToUpper(CultureInfo.CurrentCulture).Trim());
        }

        /// <summary>
        /// Saves this object to the <see cref="SavePath"/> as a JSON string.
        /// </summary>
        public void SaveToFile()
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));

            using (var fileStream = new FileStream(SavePath, FileMode.Create))
            {
                fileStream.Write(data, 0, data.Length);
                fileStream.Flush();
            }
        }

        #endregion Methods
    }
}
