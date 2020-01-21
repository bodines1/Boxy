using Newtonsoft.Json;

namespace Boxy.Model.ScryfallData
{
    public class PurchaseUris
    {
        [JsonProperty("tcgplayer")]
        public string Tcgplayer { get; set; }
        
        [JsonProperty("cardmarket")]
        public string Cardmarket { get; set; }
        
        [JsonProperty("cardhoarder")]
        public string Cardhoarder { get; set; }
    }
}
