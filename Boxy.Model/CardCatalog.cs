using Boxy.Model.ScryfallData;
using System.Collections.Generic;
using System.Linq;

namespace Boxy.Model
{
    public class CardCatalog
    {
        public static string SavePath { get; } = @"\Catalog\scryfall-oracle-cards.json";

        public BulkData CatalogBulkData { get; set; }

        public List<Card> Cards { get; set; }

        public Card FindCard(string oracleId)
        {
            return Cards.Single(c => c.OracleId == oracleId);
        }
    }
}
