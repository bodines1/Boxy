using Boxy.Model.ScryfallData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Boxy.Model.SerializedData
{
    /// <summary>
    /// Holds a mapping of Oracle Ids to Cards to store and retrieve a user's preferred printing of a card.
    /// </summary>
    public class ArtworkPreferences : Dictionary<string, Card>
    {
        #region Constructors

        /// <summary>
        /// Constructor for JSON use, normal creation must be through the <see cref="CreateFromFile"/> method.
        /// </summary>
        public ArtworkPreferences()
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
                return new ArtworkPreferences();
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
        public Card GetPreferredCard(Card card)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card), "Card cannot be null. Consumer must check object before using this method.");
            }

            if (!ContainsKey(card.OracleId))
            {
                Add(card.OracleId, card);
            }

            return this[card.OracleId];
        }

        /// <summary>
        /// Gets the card ID of the user's preferred (most recently selected) printing of a card. Stored persistently between sessions.
        /// </summary>
        public string GetPreferredCardId(Card card)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card), "Card cannot be null. Consumer must check object before using this method.");
            }

            if (!ContainsKey(card.OracleId))
            {
                Add(card.OracleId, card);
            }

            return this[card.OracleId].Id;
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

            Add(card.OracleId, card);
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
