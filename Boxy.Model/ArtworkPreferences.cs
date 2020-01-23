using Boxy.Model.ScryfallData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Boxy.Model
{
    /// <summary>
    /// Holds a mapping of Oracle Ids to Card Ids to store and retrieve a user's preferred printing of a card.
    /// </summary>
    public class ArtworkPreferences : Dictionary<string, string>
    {
        #region Constructors

        /// <summary>
        /// Private constructor, creation must be through the <see cref="CreateFromFile"/> method.
        /// </summary>
        private ArtworkPreferences()
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="ArtworkPreferences"/> by deserializing a file at the <see cref="SavePath"/> if it
        /// exists, or a new instance if deserialization fails.
        /// </summary>
        public static ArtworkPreferences CreateFromFile()
        {
            try
            {
                return JsonConvert.DeserializeObject<ArtworkPreferences>(File.ReadAllText(SavePath));
            }
            catch (Exception)
            {
                return (ArtworkPreferences)new Dictionary<string, string>();
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Path to save the serialized <see cref="ArtworkPreferences"/> file to.
        /// </summary>
        private static string SavePath { get; } = @"artwork-preferences.json";

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets the card ID of the user's preferred (most recently selected) printing of a card. Stored persistently between sessions.
        /// </summary>
        public Card GetPreferredCard(List<Card> allPrintings)
        {
            if (allPrintings == null || !allPrintings.Any())
            {
                throw new ArgumentNullException(nameof(allPrintings), "List of card objects cannot be null or empty. Consumer must check object before using this method.");
            }

            Card firstCard = allPrintings.First();

            if (allPrintings.Any(card => firstCard.OracleId != card.OracleId))
            {
                throw new InvalidOperationException("When calling GetPreferredCardId, all cards must be have a matching Oracle ID.");
            }

            if (ContainsKey(firstCard.OracleId))
            {
                return allPrintings.Single(c => c.Id == this[firstCard.OracleId]);
            }

            Add(firstCard.OracleId, firstCard.Id);
            return firstCard;

        }

        /// <summary>
        /// Updates the preference dictionary with the passed in card being the user's "preferred" version for that Oracle ID.
        /// </summary>
        /// <param name="card">The card to set as preferred.</param>
        public void UpdatePreferredCard(Card card)
        {
            if (ContainsKey(card.OracleId))
            {
                Remove(card.OracleId);
            }

            Add(card.OracleId, card.Id);
        }

        public void SaveToFile()
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));

            try
            {
                using (var fileStream = new FileStream(SavePath, FileMode.Create))
                {
                    fileStream.Write(data, 0, data.Length);
                    fileStream.Flush();
                }
            }
            catch (Exception)
            {
                // ignored, no need for special handling of a save failure. The dictionary will simply fail to 
                // deserialize on next call to CreateFromFile, which will result in a new preference file.
            }
        }

        #endregion Methods
    }
}
