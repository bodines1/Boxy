using Newtonsoft.Json;

namespace CardMimic.Model.ScryfallData
{
    public class Prices
    {
        [JsonProperty("usd")]
        public string Usd { get; set; }
        
        [JsonProperty("eur")]
        public string Eur { get; set; }
        
        [JsonProperty("tix")]
        public string Tix { get; set; }
    }
}
