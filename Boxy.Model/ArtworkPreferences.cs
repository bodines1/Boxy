using Boxy.Model.ScryfallData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Boxy.Model
{
    public static class ArtworkPreferences
    {
        /// <summary>
        /// Path to save the serialized <see cref="ArtworkPreferences"/> file to.
        /// </summary>
        private static string SavePath { get; } = @"artwork-preferences.json";

        private static Dictionary<string, string> _preferenceDictionary;

        /// <summary>
        /// The actual dictionary mapping Oracle IDs to the user's "preferred" Card ID.
        /// </summary>
        private static Dictionary<string, string> PreferenceDictionary
        {
            get
            {
                return _preferenceDictionary ?? (_preferenceDictionary = new Dictionary<string, string>());
            }
        }

        /// <summary>
        /// Gets the card ID of the user's preferred (most recently selected) printing of a card. Stored persistently between sessions.
        /// </summary>
        public static Card GetPreferredCard(List<Card> allPrintings)
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

            if (PreferenceDictionary.ContainsKey(firstCard.OracleId))
            {
                return allPrintings.Single(c => c.Id == PreferenceDictionary[firstCard.OracleId]);
            }

            PreferenceDictionary.Add(firstCard.OracleId, firstCard.Id);
            return firstCard;

        }

        /// <summary>
        /// Updates the preference dictionary with the passed in card being the user's "preferred" version for that Oracle ID.
        /// </summary>
        /// <param name="card">The card to set as preferred.</param>
        public static void UpdatePreferredCard(Card card)
        {
            if (PreferenceDictionary.ContainsKey(card.OracleId))
            {
                PreferenceDictionary.Remove(card.OracleId);
            }

            PreferenceDictionary.Add(card.OracleId, card.Id);
        }

        /// <summary>
        /// Initializes 
        /// </summary>
        public static void Initialize()
        {
            try
            {
                _preferenceDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(SavePath));
            }
            catch (Exception)
            {
                _preferenceDictionary = new Dictionary<string, string>();
            }
        }

        public static void SavePreferencesToFile()
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(PreferenceDictionary));

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
                // deserialize on next call to Initialize, which will result in a new preference file.
            }
        }
    }
}
