using Newtonsoft.Json;

namespace Boxy.Model.ScryfallData
{
    public class CardList
    {
        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("total_cards")]
        public long TotalCards { get; set; }

        [JsonProperty("has_more")]
        public bool HasMore { get; set; }

        [JsonProperty("data")]
        public Card[] Data { get; set; }
    }
}
