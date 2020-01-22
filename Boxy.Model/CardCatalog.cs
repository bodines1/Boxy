using Boxy.Model.ScryfallData;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Boxy.Model
{
    public class CardCatalog
    {
        public CardCatalog(DateTime updatedAt, List<Card> cards)
        {
            UpdatedAt = updatedAt;
            Cards = cards;
        }

        public static Uri ScryfallUri { get; } = new Uri("https://archive.scryfall.com/json/scryfall-oracle-cards.json");

        public static string SavePath { get; } = @"Catalog\scryfall-oracle-cards.json";

        public DateTime UpdatedAt { get; }

        public List<Card> Cards { get; }

        public Card FindCard(string name)
        {
            return Cards.Find(c => c.Name.ToUpper(CultureInfo.CurrentCulture).Trim() == name.ToUpper(CultureInfo.CurrentCulture).Trim());
        }
    }
}
